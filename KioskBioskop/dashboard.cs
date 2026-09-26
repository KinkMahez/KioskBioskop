using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KioskBioskop
{
    public partial class dashboard : Form
    {
        public dashboard()
        {
            InitializeComponent();
        }

        private void dashboard_Load(object sender, EventArgs e)
        {
            MuatUlangSemuaData();
        }

        // Panggil method ini dari tombol Refresh Anda, atau dari Timer, dsb.
        private void MuatUlangSemuaData()
        {
            MuatKartuRingkasan();
            MuatJadwalHariIni();
            MuatTransaksiTerbaru();
        }

        // =========================================================
        //  1. KARTU RINGKASAN (KPI)
        //     Sesuaikan nama label (lblTotalFilm, dst.) dengan Label
        //     yang sudah Anda buat sendiri di Designer.
        // =========================================================
        private void MuatKartuRingkasan()
        {
            // --- Total film sedang tayang ---
            DB.crud("SELECT COUNT(*) AS Jumlah FROM `master_film` WHERE Status_Tayang = 'NOW_SHOWING'");
            lblTotalFilm.Text = AmbilNilai(0, "Jumlah");

            // --- Total studio ---
            DB.crud("SELECT COUNT(*) AS Jumlah FROM `master_studio`");
            lblTotalStudio.Text = AmbilNilai(0, "Jumlah");

            // --- Transaksi, pendapatan, dan tiket hari ini ---
            // (transaksi berstatus CANCELLED/FAILED/GAGAL/DIBATALKAN tidak dihitung)
            DB.crud(@"SELECT
                        COUNT(*) AS JumlahTransaksi,
                        IFNULL(SUM(Total_Bayar), 0) AS TotalBayar,
                        IFNULL(SUM(Total_Tiket), 0) AS TotalTiket
                      FROM `transaksi`
                      WHERE DATE(Waktu_Transaksi) = CURDATE()
                        AND Status_Transaksi NOT IN ('CANCELLED', 'FAILED', 'GAGAL', 'DIBATALKAN')");

            lblTransaksiHariIni.Text = AmbilNilai(0, "JumlahTransaksi");

            decimal totalBayar = 0;
            decimal.TryParse(AmbilNilai(0, "TotalBayar"), out totalBayar);
            lblPendapatanHariIni.Text = "Rp " + totalBayar.ToString("N0");

            lblTiketTerjualHariIni.Text = AmbilNilai(0, "TotalTiket");
        }

        // =========================================================
        //  2. TABEL: JADWAL TAYANG HARI INI
        //     Sesuaikan nama dgvJadwalHariIni dan urutan kolomnya
        //     dengan DataGridView yang Anda buat sendiri.
        //     Kolom yang diharapkan (5 kolom, berurutan):
        //     Judul Film | Studio | Jam | Harga | Kursi Terjual
        // =========================================================
        private void MuatJadwalHariIni()
        {
            dgvJadwalHariIni.Rows.Clear();

            DB.crud(@"SELECT
                        f.Judul,
                        s.Nama_Studio,
                        j.Jam_Mulai,
                        j.Harga_Tiket,
                        (SELECT COUNT(*) FROM transaksi_detail td
                            INNER JOIN transaksi t ON t.Transaksi_ID = td.Transaksi_ID
                            WHERE td.Jadwal_ID = j.Jadwal_ID
                              AND t.Status_Transaksi NOT IN ('CANCELLED', 'FAILED', 'GAGAL', 'DIBATALKAN')
                        ) AS KursiTerjual
                      FROM `jadwal_tayang` j
                      INNER JOIN `master_film` f ON f.Film_ID = j.Film_ID
                      INNER JOIN `master_studio` s ON s.Studio_ID = j.Studio_ID
                      WHERE j.Tanggal_Tayang = CURDATE()
                      ORDER BY j.Jam_Mulai ASC");

            foreach (DataRow r in DB.ds.Tables[0].Rows)
            {
                string jam = FormatJam(r["Jam_Mulai"]);
                decimal harga = r["Harga_Tiket"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Harga_Tiket"]);

                dgvJadwalHariIni.Rows.Add(
                    r["Judul"].ToString(),
                    r["Nama_Studio"].ToString(),
                    jam,
                    "Rp " + harga.ToString("N0"),
                    r["KursiTerjual"].ToString()
                );
            }
        }

        // =========================================================
        //  3. TABEL: TRANSAKSI TERBARU
        //     Sesuaikan nama dgvTransaksiTerbaru dan urutan kolomnya.
        //     Kolom yang diharapkan (5 kolom, berurutan):
        //     Kode Transaksi | Waktu | Jml Tiket | Total Bayar | Status
        // =========================================================
        private void MuatTransaksiTerbaru()
        {
            dgvTransaksiTerbaru.Rows.Clear();

            DB.crud(@"SELECT Kode_Transaksi, Waktu_Transaksi, Total_Tiket, Total_Bayar, Status_Transaksi
                      FROM `transaksi`
                      ORDER BY Transaksi_ID DESC
                      LIMIT 10");

            foreach (DataRow r in DB.ds.Tables[0].Rows)
            {
                decimal bayar = r["Total_Bayar"] == DBNull.Value ? 0 : Convert.ToDecimal(r["Total_Bayar"]);
                DateTime waktu = r["Waktu_Transaksi"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["Waktu_Transaksi"]);

                dgvTransaksiTerbaru.Rows.Add(
                    r["Kode_Transaksi"].ToString(),
                    waktu == DateTime.MinValue ? "-" : waktu.ToString("dd/MM/yyyy HH:mm"),
                    r["Total_Tiket"].ToString(),
                    "Rp " + bayar.ToString("N0"),
                    r["Status_Transaksi"].ToString()
                );
            }
        }

        // =========================================================
        //  HELPER
        // =========================================================
        private string AmbilNilai(int rowIndex, string kolom)
        {
            if (DB.ds.Tables.Count == 0 || DB.ds.Tables[0].Rows.Count <= rowIndex) return "0";
            var val = DB.ds.Tables[0].Rows[rowIndex][kolom];
            return val == DBNull.Value ? "0" : val.ToString();
        }

        private static string FormatJam(object jamValue)
        {
            if (jamValue == null || jamValue == DBNull.Value) return "-";
            if (jamValue is TimeSpan ts) return ts.ToString(@"hh\:mm");
            if (jamValue is DateTime dt) return dt.ToString("HH:mm");

            TimeSpan parsed;
            if (TimeSpan.TryParse(jamValue.ToString(), out parsed))
                return parsed.ToString(@"hh\:mm");

            return jamValue.ToString();
        }

        private void dgvJadwalHariIni_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        // Panggil MuatUlangSemuaData() ini dari tombol Refresh Anda sendiri, contoh:
        // private void btnRefresh_Click(object sender, EventArgs e) => MuatUlangSemuaData();
    }
}