using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KioskBioskop
{
    public partial class film : Form
    {

        private string connString = "Server=localhost;Port=3306;Database=dbkiostiket;Uid=root;Pwd=;";
        public film()
        {
            InitializeComponent();
        }

        public void clear()
        {
            txtJudul.Text = "";
            txtSinopsis.Text = "";
            txtGenre.Text = "";
            txtPosterPath.Text = "";
            cmbRatingUsia.SelectedIndex = -1 ;
            cmbStatus.SelectedIndex = -1;
            numDurasi.Value = 0;
            picPoster.Image = null;
        }

        public void TampilData()
        {
            // 1. Bersihkan baris lama di DataGridView
            dgvFilm.Rows.Clear();

            // 2. Eksekusi query SELECT kolom spesifik saja
            DB.crud("SELECT Film_ID, Judul, Genre, Durasi_Menit, Rating_Usia, Status_Tayang FROM `master_film` ORDER BY Film_ID DESC");

            // 3. Looping setiap baris data dan masukkan ke DataGridView
            foreach (DataRow baris in DB.ds.Tables[0].Rows)
            {
                string id = "" + baris["Film_ID"];
                string judul = "" + baris["Judul"];
                string genre = "" + baris["Genre"];
                string durasi = "" + baris["Durasi_Menit"];
                string rating = "" + baris["Rating_Usia"];
                string status = "" + baris["Status_Tayang"];

                // Tambahkan data ke kolom DataGridView secara berurutan
                dgvFilm.Rows.Add(id, judul, genre, durasi, rating, status);
            }
        }

        private void guna2Shapes1_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void film_Load(object sender, EventArgs e)
        {

        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtJudul.Text))
            {
                MessageBox.Show("Judul Film tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"INSERT INTO `master_film` 
                           (Judul, Sinopsis, Genre, Durasi_Menit, Rating_Usia, Poster_Path, Status_Tayang) 
                           VALUES 
                           (@judul, @sinopsis, @genre, @durasi, @rating, @poster, @status)";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@judul", txtJudul.Text.Trim());
                        cmd.Parameters.AddWithValue("@sinopsis", txtSinopsis.Text.Trim());
                        cmd.Parameters.AddWithValue("@genre", txtGenre.Text.Trim());
                        cmd.Parameters.AddWithValue("@durasi", Convert.ToInt32(numDurasi.Value));
                        cmd.Parameters.AddWithValue("@rating", cmbRatingUsia.Text);
                        cmd.Parameters.AddWithValue("@poster", txtPosterPath.Text.Trim());
                        cmd.Parameters.AddWithValue("@status", cmbStatus.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Data Film berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        clear();
                        TampilData();

                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBrowsePoster_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtPosterPath.Text = ofd.FileName;
                picPoster.ImageLocation = ofd.FileName;
            }
        }

        private void txtSinopsis_TextChanged(object sender, EventArgs e)
        {

        }

        private void dgvFilm_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvFilm_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int baris = e.RowIndex;
            int kolom = e.ColumnIndex;

            // Mencegah error jika user mengklik Header Tabel
            if (baris < 0) return;

            // KOLOM EDIT (Misal ada di indeks kolom ke-6)
            if (kolom == 6)
            {
                // Ambil Film_ID dari sel pertama (Cell 0) pada baris yang diklik
                string idFilm = dgvFilm.Rows[baris].Cells[0].Value.ToString();

                // Query untuk ambil data lengkap film tersebut
                DB.crud($"SELECT * FROM `master_film` WHERE Film_ID = '{idFilm}'");

                foreach (DataRow brs in DB.ds.Tables[0].Rows)
                {
                    string id = "" + brs["Film_ID"];
                    string judul = "" + brs["Judul"];
                    string sinopsis = "" + brs["Sinopsis"];
                    string genre = "" + brs["Genre"];
                    string durasi = "" + brs["Durasi_Menit"];
                    string rating = "" + brs["Rating_Usia"];
                    string poster = "" + brs["Poster_Path"];
                    string status = "" + brs["Status_Tayang"];

                    // Isi nilai ke masing-masing inputan Form
                    txtID.Text = id;
                    txtJudul.Text = judul;
                    txtSinopsis.Text = sinopsis;
                    txtGenre.Text = genre;
                    numDurasi.Value = Convert.ToDecimal(durasi);
                    cmbRatingUsia.Text = rating;
                    txtPosterPath.Text = poster;
                    cmbStatus.Text = status;

                    // Tampilkan preview poster jika filenya ada di komputer
                    if (System.IO.File.Exists(poster))
                    {
                        picPoster.ImageLocation = poster;
                    }
                    else
                    {
                        picPoster.Image = null;
                    }
                }
            }

            // KOLOM HAPUS (Misal ada di indeks kolom ke-7)
            if (kolom == 7)
            {
                string idFilm = dgvFilm.Rows[baris].Cells[0].Value.ToString();

                DialogResult setuju = MessageBox.Show("Apakah Anda yakin ingin menghapus film dengan ID " + idFilm + " ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (setuju == DialogResult.Yes)
                {
                    DB.crud($"DELETE FROM `master_film` WHERE Film_ID = '{idFilm}'");
                    MessageBox.Show("Data berhasil dihapus!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh tabel
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
            string query = $@"UPDATE `master_film` SET 
                    Judul = '{txtJudul.Text}', 
                    Sinopsis = '{txtSinopsis.Text}', 
                    Genre = '{txtGenre.Text}', 
                    Durasi_Menit = '{numDurasi.Value}', 
                    Rating_Usia = '{cmbRatingUsia.Text}', 
                    Poster_Path = '{txtPosterPath.Text}', 
                    Status_Tayang = '{cmbStatus.Text}' 
                    WHERE Film_ID = '{txtID.Text}'";

            DB.crud(query);
            MessageBox.Show("Data Film berhasil diperbarui!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Refresh DataGridView & Reset Input
            TampilData();
            clear();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            TampilData();
        }
    }
}
