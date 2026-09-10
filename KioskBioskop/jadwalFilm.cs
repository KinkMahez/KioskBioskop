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
    public partial class jadwalFilm : Form
    {
        public jadwalFilm()
        {
            InitializeComponent();

        }


        private void LoadComboBoxData()
        {
            // Load Data Film
            cmbFilm.Items.Clear();
            DB.crud("SELECT Film_ID, Judul FROM `master_film` WHERE Status_Tayang = 'Now Showing'");
            DataTable dtFilm = DB.ds.Tables[0];
            cmbFilm.DataSource = dtFilm;
            cmbFilm.DisplayMember = "Judul";
            cmbFilm.ValueMember = "Film_ID";

            // Load Data Studio
            DB.crud("SELECT Studio_ID, Nama_Studio FROM `master_studio`");
            DataTable dtStudio = DB.ds.Tables[0];
            cmbStudio.DataSource = dtStudio;
            cmbStudio.DisplayMember = "Nama_Studio";
            cmbStudio.ValueMember = "Studio_ID";
        }

        public void TampilData()
        {
            dgvJadwal.Rows.Clear();

            // Menggunakan INNER JOIN agar yang tampil di tabel adalah Judul Film dan Nama Studio (bukan sekadar ID)
            string query = @"SELECT j.Jadwal_ID, f.Judul, s.Nama_Studio, j.Tanggal_Tayang, j.Jam_Mulai, j.Harga_Tiket 
                     FROM `jadwal_tayang` j
                     JOIN `master_film` f ON j.Film_ID = f.Film_ID
                     JOIN `master_studio` s ON j.Studio_ID = s.Studio_ID
                     ORDER BY j.Jadwal_ID DESC";

            DB.crud(query);

            foreach (DataRow baris in DB.ds.Tables[0].Rows)
            {
                string id = "" + baris["Jadwal_ID"];
                string judul = "" + baris["Judul"];
                string studio = "" + baris["Nama_Studio"];
                string tanggal = Convert.ToDateTime(baris["Tanggal_Tayang"]).ToString("yyyy-MM-dd");
                string jam = "" + baris["Jam_Mulai"];
                string harga = Convert.ToDecimal(baris["Harga_Tiket"]).ToString("N0");

                dgvJadwal.Rows.Add(id, judul, studio, tanggal, jam, harga);
            }
        }





        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void jadwalFilm_Load(object sender, EventArgs e)
        {
            LoadComboBoxData();
        }

        private void btnSimpan_Click_1(object sender, EventArgs e)
        {
            if (cmbFilm.SelectedValue == null || cmbStudio.SelectedValue == null || string.IsNullOrWhiteSpace(txtHarga.Text))
            {
                MessageBox.Show("Harap lengkapi semua data jadwal!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filmId = cmbFilm.SelectedValue.ToString();
            string studioId = cmbStudio.SelectedValue.ToString();
            string tanggal = dtpTanggal.Value.ToString("yyyy-MM-dd");
            string jam = dtpJam.Value.ToString("HH:mm:ss");
            string harga = txtHarga.Text.Trim();

            string query = $@"INSERT INTO `jadwal_tayang` (Film_ID, Studio_ID, Tanggal_Tayang, Jam_Mulai, Harga_Tiket)
                     VALUES ('{filmId}', '{studioId}', '{tanggal}', '{jam}', '{harga}')";

            DB.crud(query);
            MessageBox.Show("Jadwal tayang berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

            TampilData();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            TampilData();
        }

        private void cmbFilm_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Pastikan ada item yang dipilih dan SelectedValue bukan data temporer saat awal loading
            if (cmbFilm.SelectedValue == null || cmbFilm.SelectedValue is DataRowView) return;

            string filmId = cmbFilm.SelectedValue.ToString();

            // Query untuk mengambil lokasi file poster dari database
            DB.crud($"SELECT Poster_Path FROM `master_film` WHERE Film_ID = '{filmId}'");

            if (DB.ds.Tables[0].Rows.Count > 0)
            {
                string pathGambar = "" + DB.ds.Tables[0].Rows[0]["Poster_Path"];

                // Cek apakah path file gambar benar-benar ada di komputer
                if (!string.IsNullOrWhiteSpace(pathGambar) && System.IO.File.Exists(pathGambar))
                {
                    picPoster.ImageLocation = pathGambar;
                    picPoster.SizeMode = PictureBoxSizeMode.StretchImage; // Agar gambar pas dengan ukuran box
                }
                else
                {
                    // Jika file gambar tidak ditemukan atau kosong, kosongkan PictureBox
                    picPoster.Image = null;
                }
            }
        }

        private void dgvJadwal_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int baris = e.RowIndex;
            int kolom = e.ColumnIndex;

            // Mencegah error jika user mengklik Header Tabel atau baris kosong
            if (baris < 0 || dgvJadwal.Rows[baris].IsNewRow) return;

            // KOLOM EDIT (Misal ada di indeks kolom ke-6)
            if (kolom == 6)
            {
                // Ambil Jadwal_ID dari Cell 0
                string idJadwal = dgvJadwal.Rows[baris].Cells[0].Value?.ToString();

                if (string.IsNullOrEmpty(idJadwal)) return;

                // Query untuk mengambil data lengkap dari tabel jadwal_tayang
                DB.crud($"SELECT * FROM `jadwal_tayang` WHERE Jadwal_ID = '{idJadwal}'");

                if (DB.ds.Tables[0].Rows.Count > 0)
                {
                    DataRow brs = DB.ds.Tables[0].Rows[0];

                    string id = "" + brs["Jadwal_ID"];
                    string filmId = "" + brs["Film_ID"];
                    string studioId = "" + brs["Studio_ID"];
                    string tgl = "" + brs["Tanggal_Tayang"];
                    string jam = "" + brs["Jam_Mulai"];
                    string harga = "" + brs["Harga_Tiket"];

                    // 1. Set ID ke TextBox Hidden/ReadOnly
                    txtID.Text = id;

                    // 2. Set pilihan ComboBox berdasarkan ID
                    cmbFilm.SelectedValue = filmId;
                    cmbStudio.SelectedValue = studioId;

                    // 3. Set Tanggal dan Jam ke DateTimePicker
                    if (DateTime.TryParse(tgl, out DateTime tglResult))
                    {
                        dtpTanggal.Value = tglResult;
                    }

                    if (DateTime.TryParse(jam, out DateTime jamResult))
                    {
                        dtpJam.Value = jamResult;
                    }

                    // 4. Set Harga Tiket
                    txtHarga.Text = harga;
                }
            }

            // KOLOM HAPUS (Misal ada di indeks kolom ke-7)
            if (kolom == 7)
            {
                string idJadwal = dgvJadwal.Rows[baris].Cells[0].Value?.ToString();

                if (string.IsNullOrEmpty(idJadwal)) return;

                DialogResult setuju = MessageBox.Show($"Apakah Anda yakin ingin menghapus jadwal dengan ID {idJadwal}?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (setuju == DialogResult.Yes)
                {
                    DB.crud($"DELETE FROM `jadwal_tayang` WHERE Jadwal_ID = '{idJadwal}'");
                    MessageBox.Show("Jadwal tayang berhasil dihapus!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh DataGridView
                    TampilData();
                }
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text))
            {
                MessageBox.Show("Pilih film yang ingin diubah terlebih dahulu dari tabel!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Query UPDATE

            string filmId = cmbFilm.SelectedValue.ToString();
            string studioId = cmbStudio.SelectedValue.ToString();
            string tanggal = dtpTanggal.Value.ToString("yyyy-MM-dd");
            string jam = dtpJam.Value.ToString("HH:mm:ss");
            string harga = txtHarga.Text.Trim();

            string query = $@"UPDATE `jadwal_tayang` SET 
                    Film_ID = '{filmId}', 
                    Studio_ID = '{studioId}', 
                    Tanggal_Tayang = '{tanggal}', 
                    Jam_Mulai = '{jam}', 
                    Harga_Tiket = '{harga}'
                    WHERE Jadwal_ID = '{txtID.Text}'";

            DB.crud(query);
            MessageBox.Show("Data Film berhasil diperbarui!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Refresh DataGridView & Reset Input
            TampilData();
        }
    }
}
