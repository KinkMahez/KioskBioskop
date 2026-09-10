using Org.BouncyCastle.Asn1.Cmp;
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
    public partial class studio : Form
    {
        public studio()
        {
            InitializeComponent();
        }

        public void TampilData()
        {
            // 1. Bersihkan baris lama di DataGridView
            dgvFilm.Rows.Clear();

            // 2. Eksekusi query SELECT kolom spesifik saja
            DB.crud("SELECT * FROM `master_studio` ORDER BY Studio_ID DESC");

            // 3. Looping setiap baris data dan masukkan ke DataGridView
            foreach (DataRow baris in DB.ds.Tables[0].Rows)
            {
                string id = "" + baris["Studio_ID"];
                string judul = "" + baris["Nama_Studio"];
                string genre = "" + baris["Tipe_Studio"];
                string durasi = "" + baris["Jumlah_Baris"];
                string rating = "" + baris["Jumlah_Kolom"];


                // Tambahkan data ke kolom DataGridView secara berurutan
                dgvFilm.Rows.Add(id, judul, genre, durasi, rating);
            }
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNamaStudio.Text))
            {
                MessageBox.Show("Nama Studio tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int jumlahBaris = (int)numBaris.Value;
            int jumlahKolom = (int)numKolom.Value;

            if (jumlahBaris <= 0 || jumlahKolom <= 0)
            {
                MessageBox.Show("Jumlah Baris dan Jumlah Kolom harus lebih dari 0 agar kursi bisa dibuat otomatis!",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = $@"INSERT INTO `master_studio` (Nama_Studio, Tipe_Studio, Jumlah_Baris, Jumlah_Kolom) 
                     VALUES ('{txtNamaStudio.Text.Trim()}', '{cmbTipeStudio.Text}', '{numBaris.Value}', '{numKolom.Value}')";

            DB.crud(query);

            // Ambil Studio_ID yang baru saja dibuat (studio terakhir yang diinsert)
            int newStudioId = AmbilStudioIdTerbaru();

            if (newStudioId > 0)
            {
                int jumlahKursiDibuat = GenerateKursiOtomatis(newStudioId, jumlahBaris, jumlahKolom);
                MessageBox.Show(
                    $"Data Studio berhasil disimpan!\n{jumlahKursiDibuat} kursi otomatis dibuat ({jumlahBaris} baris x {jumlahKolom} kolom).",
                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Data Studio berhasil disimpan, tetapi gagal membuat kursi otomatis (Studio_ID tidak ditemukan).",
                    "Sukses (sebagian)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            TampilData();
        }

        private void dgvFilm_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int baris = e.RowIndex;
            int kolom = e.ColumnIndex;

            // Mencegah error jika user mengklik Header Tabel
            if (baris < 0) return;

            // KOLOM EDIT (Misal ada di indeks kolom ke-6)
            if (kolom == 5)
            {
                // Ambil Film_ID dari sel pertama (Cell 0) pada baris yang diklik
                string idFilm = dgvFilm.Rows[baris].Cells[0].Value.ToString();

                // Query untuk ambil data lengkap film tersebut
                DB.crud($"SELECT * FROM `master_studio` WHERE Studio_ID = '{idFilm}'");

                foreach (DataRow brs in DB.ds.Tables[0].Rows)
                {
                    string id = "" + brs["Studio_ID"];
                    string nama = "" + brs["Nama_Studio"];
                    string tipe = "" + brs["Tipe_Studio"];
                    string bars = "" + brs["Jumlah_Baris"];
                    string klm = "" + brs["Jumlah_Kolom"];

                    // Isi nilai ke masing-masing inputan Form
                    txtID.Text = id;
                    txtNamaStudio.Text = nama;
                    cmbTipeStudio.Text = tipe;
                    numBaris.Value = Convert.ToDecimal(bars);
                    numKolom.Value = Convert.ToDecimal(klm);


                }
            }

            // KOLOM HAPUS (Misal ada di indeks kolom ke-7)
            if (kolom == 6)
            {
                string idFilm = dgvFilm.Rows[baris].Cells[0].Value.ToString();

                DialogResult setuju = MessageBox.Show("Apakah Anda yakin ingin menghapus Studio dengan ID " + idFilm + " ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (setuju == DialogResult.Yes)
                {
                    // Hapus dulu kursi milik studio ini (kalau ada) supaya tidak melanggar foreign key,
                    // selama belum pernah dipakai transaksi
                    try
                    {
                        DB.crud($"DELETE FROM `master_kursi` WHERE Studio_ID = '{idFilm}'");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Studio tidak bisa dihapus karena kursinya sudah pernah dipakai transaksi.\n\nDetail: " + ex.Message,
                            "Gagal Menghapus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DB.crud($"DELETE FROM `master_studio` WHERE Studio_ID = '{idFilm}'");
                    MessageBox.Show("Data berhasil dihapus!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh tabel
                    TampilData();
                }
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            TampilData();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih film yang ingin diubah terlebih dahulu dari tabel!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studioId = Convert.ToInt32(txtID.Text);
            int jumlahBaris = (int)numBaris.Value;
            int jumlahKolom = (int)numKolom.Value;

            // Query UPDATE
            string query = $@"UPDATE `master_studio` SET 
                    Nama_Studio = '{txtNamaStudio.Text}', 
                    Tipe_Studio = '{cmbTipeStudio.Text}', 
                    Jumlah_Baris = '{numBaris.Value}', 
                    Jumlah_Kolom = '{numKolom.Value}' 
                    WHERE Studio_ID = '{txtID.Text}'";

            DB.crud(query);

            // Cek apakah studio ini sudah punya kursi
            int jumlahKursiSekarang = HitungKursi(studioId);

            if (jumlahKursiSekarang == 0 && jumlahBaris > 0 && jumlahKolom > 0)
            {
                // Belum ada kursi sama sekali -> langsung generate otomatis
                int dibuat = GenerateKursiOtomatis(studioId, jumlahBaris, jumlahKolom);
                MessageBox.Show(
                    $"Data Studio berhasil diperbarui!\n{dibuat} kursi otomatis dibuat ({jumlahBaris} baris x {jumlahKolom} kolom).",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (jumlahBaris > 0 && jumlahKolom > 0)
            {
                // Sudah ada kursi -> tanya dulu sebelum menghapus & generate ulang
                DialogResult jawab = MessageBox.Show(
                    $"Studio ini sudah punya {jumlahKursiSekarang} kursi.\n" +
                    $"Hapus kursi lama dan buat ulang sesuai {jumlahBaris} baris x {jumlahKolom} kolom yang baru?\n\n" +
                    "Catatan: kursi yang sudah pernah dipakai transaksi tidak bisa dihapus.",
                    "Regenerate Kursi?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (jawab == DialogResult.Yes)
                {
                    try
                    {
                        DB.crud($"DELETE FROM `master_kursi` WHERE Studio_ID = '{studioId}'");
                        int dibuat = GenerateKursiOtomatis(studioId, jumlahBaris, jumlahKolom);
                        MessageBox.Show(
                            $"Data Studio berhasil diperbarui!\nKursi lama dihapus, {dibuat} kursi baru dibuat.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Data Studio berhasil diperbarui, tetapi kursi lama gagal dihapus/diganti " +
                            "(kemungkinan sudah dipakai transaksi).\n\nDetail: " + ex.Message,
                            "Success (sebagian)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Data Studio berhasil diperbarui! (Kursi lama tidak diubah)",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Data Studio berhasil diperbarui!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Refresh DataGridView & Reset Input
            TampilData();
        }

        private void dgvFilm_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void studio_Load(object sender, EventArgs e)
        {

        }

        // =========================================================
        //  FITUR AUTO-GENERATE KURSI (master_kursi)
        // =========================================================

        /// <summary>
        /// Ambil Studio_ID yang baru saja diinsert (studio dengan ID tertinggi).
        /// </summary>
        private int AmbilStudioIdTerbaru()
        {
            DB.crud("SELECT Studio_ID FROM `master_studio` ORDER BY Studio_ID DESC LIMIT 1");

            if (DB.ds.Tables[0].Rows.Count > 0)
                return Convert.ToInt32(DB.ds.Tables[0].Rows[0]["Studio_ID"]);

            return 0;
        }

        /// <summary>
        /// Hitung berapa kursi yang sudah ada di master_kursi untuk 1 studio.
        /// </summary>
        private int HitungKursi(int studioId)
        {
            DB.crud($"SELECT COUNT(*) AS Jumlah FROM `master_kursi` WHERE Studio_ID = '{studioId}'");

            if (DB.ds.Tables[0].Rows.Count > 0)
                return Convert.ToInt32(DB.ds.Tables[0].Rows[0]["Jumlah"]);

            return 0;
        }

        /// <summary>
        /// Generate baris kursi ke master_kursi sesuai jumlah baris & kolom.
        /// Baris kursi diberi kode huruf (A, B, C, ... Z, AA, AB, ...),
        /// kolom diberi nomor urut 1..jumlahKolom, dan Label_Kursi = Kode_Baris + Nomor_Kolom (mis. A1, A2, B1, ...).
        /// Status_Fisik diset 'ACTIVE' untuk semua kursi baru.
        /// Mengembalikan jumlah kursi yang berhasil dibuat.
        /// </summary>
        private int GenerateKursiOtomatis(int studioId, int jumlahBaris, int jumlahKolom)
        {
            var values = new List<string>();

            for (int b = 1; b <= jumlahBaris; b++)
            {
                string kodeBaris = RowIndexToLetter(b);

                for (int k = 1; k <= jumlahKolom; k++)
                {
                    string labelKursi = kodeBaris + k;
                    values.Add($"('{studioId}', '{kodeBaris}', '{k}', '{labelKursi}', 'ACTIVE')");
                }
            }

            if (values.Count == 0) return 0;

            // Insert sekaligus dalam batch (per 500 baris) supaya query tidak terlalu panjang
            const int batchSize = 500;
            int totalDibuat = 0;

            for (int i = 0; i < values.Count; i += batchSize)
            {
                var batch = values.Skip(i).Take(batchSize);
                string query = "INSERT INTO `master_kursi` (Studio_ID, Kode_Baris, Nomor_Kolom, Label_Kursi, Status_Fisik) VALUES "
                                + string.Join(", ", batch);

                DB.crud(query);
                totalDibuat += batch.Count();
            }

            return totalDibuat;
        }

        /// <summary>
        /// Ubah 1 -> "A", 2 -> "B", ... 26 -> "Z", 27 -> "AA", 28 -> "AB", dst,
        /// supaya jumlah baris kursi tidak terbatas 26.
        /// </summary>
        private static string RowIndexToLetter(int index)
        {
            string result = "";
            while (index > 0)
            {
                int rem = (index - 1) % 26;
                result = (char)('A' + rem) + result;
                index = (index - 1) / 26;
            }
            return result;
        }
    }
}