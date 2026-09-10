using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace KioskBioskop
{
    public static class DatabaseHelper
    {
        // Helper internal untuk memastikan koneksi dalam kondisi terbuka
        private static void OpenConnection()
        {
            if (DB.koneksi.State == ConnectionState.Closed)
            {
                DB.koneksi.Open();
            }
        }

        #region Authentication & Admin

        public static DataRow ValidateLogin(string username, string password)
        {
            OpenConnection();
            string sql = $"SELECT User_ID, Username, Role, Nama_Lengkap FROM master_user WHERE Username = '{username}' AND Password = '{password}' AND Is_Active = 1";

            DB.crud(sql);

            if (DB.ds.Tables.Count > 0 && DB.ds.Tables[0].Rows.Count > 0)
            {
                return DB.ds.Tables[0].Rows[0];
            }
            return null;
        }

        #endregion

        #region Master Data (Film & Studio)

        public static DataTable GetFilms(string genreFilter = null)
        {
            // Menggunakan LOWER() dan UPPER() agar tidak sensitif terhadap huruf besar/kecil
            string sql = "SELECT Film_ID, Judul, Sinopsis, Genre, Durasi_Menit, Rating_Usia, Poster_Path, Status_Tayang " +
                         "FROM master_film " +
                         "WHERE UPPER(Status_Tayang) IN ('NOW_SHOWING', 'NOW SHOWING', '1', 'TAYANG')";

            if (!string.IsNullOrEmpty(genreFilter) && genreFilter != "Semua Genre")
            {
                sql += $" AND LOWER(Genre) LIKE '%{genreFilter.ToLower()}%'";
            }

            sql += " ORDER BY Judul ASC";

            DB.crud(sql);
            return DB.ds.Tables.Count > 0 ? DB.ds.Tables[0].Copy() : new DataTable();
        }

        public static List<string> GetDistinctGenres()
        {
            var genres = new HashSet<string>();
            string sql = "SELECT DISTINCT Genre FROM master_film WHERE Genre IS NOT NULL AND Genre != ''";

            DB.crud(sql);

            if (DB.ds.Tables.Count > 0)
            {
                foreach (DataRow row in DB.ds.Tables[0].Rows)
                {
                    var raw = row["Genre"]?.ToString() ?? "";
                    foreach (var g in raw.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = g.Trim();
                        if (trimmed.Length > 0)
                            genres.Add(trimmed);
                    }
                }
            }

            var list = new List<string> { "Semua Genre" };
            list.AddRange(genres.OrderBy(x => x));
            return list;
        }

        public static DataTable GetStudios()
        {
            string sql = "SELECT Studio_ID, Nama_Studio, Tipe_Studio, Jumlah_Baris, Jumlah_Kolom FROM master_studio";
            DB.crud(sql);
            return DB.ds.Tables.Count > 0 ? DB.ds.Tables[0].Copy() : new DataTable();
        }

        public static DataRow GetStudioById(int studioId)
        {
            string sql = $"SELECT Studio_ID, Nama_Studio, Tipe_Studio, Jumlah_Baris, Jumlah_Kolom FROM master_studio WHERE Studio_ID = {studioId}";
            DB.crud(sql);

            if (DB.ds.Tables.Count > 0 && DB.ds.Tables[0].Rows.Count > 0)
            {
                return DB.ds.Tables[0].Rows[0];
            }
            return null;
        }

        #endregion

        #region Jadwal & Kursi

        public static DataTable GetJadwalByFilm(int filmId)
        {
            string sql = $@"SELECT j.Jadwal_ID, j.Film_ID, j.Studio_ID, s.Nama_Studio,
                                   j.Tanggal_Tayang, j.Jam_Mulai, j.Harga_Tiket
                            FROM jadwal_tayang j
                            LEFT JOIN master_studio s ON s.Studio_ID = j.Studio_ID
                            WHERE j.Film_ID = {filmId}
                              AND j.Tanggal_Tayang >= CURDATE()
                            ORDER BY j.Tanggal_Tayang ASC, j.Jam_Mulai ASC";

            DB.crud(sql);
            return DB.ds.Tables.Count > 0 ? DB.ds.Tables[0].Copy() : new DataTable();
        }

        public static DataTable GetKursiByStudio(int studioId)
        {
            string sql = $@"SELECT Kursi_ID, Studio_ID, Kode_Baris, Nomor_Kolom, Label_Kursi, Status_Fisik
                            FROM master_kursi
                            WHERE Studio_ID = {studioId}
                            ORDER BY Kode_Baris ASC, Nomor_Kolom ASC";

            DB.crud(sql);
            return DB.ds.Tables.Count > 0 ? DB.ds.Tables[0].Copy() : new DataTable();
        }

        public static HashSet<int> GetBookedKursiIds(int jadwalId)
        {
            var result = new HashSet<int>();
            string sql = $@"SELECT td.Kursi_ID
                            FROM transaksi_detail td
                            INNER JOIN transaksi t ON t.Transaksi_ID = td.Transaksi_ID
                            WHERE td.Jadwal_ID = {jadwalId}
                              AND t.Status_Transaksi NOT IN ('CANCELLED', 'FAILED', 'GAGAL', 'DIBATALKAN')";

            DB.crud(sql);

            if (DB.ds.Tables.Count > 0)
            {
                foreach (DataRow row in DB.ds.Tables[0].Rows)
                {
                    result.Add(Convert.ToInt32(row["Kursi_ID"]));
                }
            }

            return result;
        }

        #endregion

        #region Transaksi

        public static TransaksiResult CreateTransaksi(string kioskId, int jadwalId, List<int> kursiIds, decimal hargaSatuan)
        {
            if (kursiIds == null || kursiIds.Count == 0)
                throw new InvalidOperationException("Tidak ada kursi yang dipilih.");

            OpenConnection();

            string kodeTransaksi = "TRX" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
            int totalTiket = kursiIds.Count;
            decimal totalBayar = hargaSatuan * totalTiket;
            string kiosk = string.IsNullOrEmpty(kioskId) ? "KIOSK-01" : kioskId;

            using (var trx = DB.koneksi.BeginTransaction())
            {
                try
                {
                    // 1. Cek bentrok kursi
                    string idsJoined = string.Join(",", kursiIds);
                    string checkSql = $@"SELECT td.Kursi_ID
                                        FROM transaksi_detail td
                                        INNER JOIN transaksi t ON t.Transaksi_ID = td.Transaksi_ID
                                        WHERE td.Jadwal_ID = {jadwalId}
                                          AND td.Kursi_ID IN ({idsJoined})
                                          AND t.Status_Transaksi NOT IN ('CANCELLED', 'FAILED', 'GAGAL', 'DIBATALKAN')";

                    using (var checkCmd = new MySqlCommand(checkSql, DB.koneksi, trx))
                    {
                        using (var reader = checkCmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Close();
                                throw new InvalidOperationException("Salah satu kursi yang dipilih baru saja dipesan orang lain.");
                            }
                        }
                    }

                    // 2. Insert Header Transaksi
                    int transaksiId;
                    string insertTrxSql = $@"INSERT INTO transaksi 
                        (Kode_Transaksi, Kiosk_ID, Waktu_Transaksi, Total_Tiket, Total_Bayar, Status_Transaksi)
                        VALUES ('{kodeTransaksi}', '{kiosk}', NOW(), {totalTiket}, {totalBayar.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 'SUCCESS');
                        SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(insertTrxSql, DB.koneksi, trx))
                    {
                        transaksiId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 3. Insert Detail Tiket
                    var tikets = new List<TiketInfo>();
                    foreach (var kursiId in kursiIds)
                    {
                        string kodeQr = "QR-" + kodeTransaksi + "-" + kursiId + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                        string insertDetailSql = $@"INSERT INTO transaksi_detail 
                            (Transaksi_ID, Jadwal_ID, Kursi_ID, Harga_Satuan, Kode_Tiket_QR)
                            VALUES ({transaksiId}, {jadwalId}, {kursiId}, {hargaSatuan.ToString(System.Globalization.CultureInfo.InvariantCulture)}, '{kodeQr}')";

                        using (var cmd = new MySqlCommand(insertDetailSql, DB.koneksi, trx))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        tikets.Add(new TiketInfo { KursiId = kursiId, KodeTiketQR = kodeQr });
                    }

                    trx.Commit();

                    return new TransaksiResult
                    {
                        TransaksiId = transaksiId,
                        KodeTransaksi = kodeTransaksi,
                        TotalTiket = totalTiket,
                        TotalBayar = totalBayar,
                        Tikets = tikets
                    };
                }
                catch
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        #endregion
    }

    #region Models

    public class TransaksiResult
    {
        public int TransaksiId { get; set; }
        public string KodeTransaksi { get; set; }
        public int TotalTiket { get; set; }
        public decimal TotalBayar { get; set; }
        public List<TiketInfo> Tikets { get; set; }
    }

    public class TiketInfo
    {
        public int KursiId { get; set; }
        public string KodeTiketQR { get; set; }
    }

    #endregion
}