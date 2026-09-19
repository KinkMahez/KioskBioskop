using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace KioskBioskop
{
    public class FormKatalog : Form
    {
        // ===== Palet warna sesuai desain =====
        private readonly Color ColBackground = ColorTranslator.FromHtml("#12182B");
        private readonly Color ColHeader = ColorTranslator.FromHtml("#171E33");
        private readonly Color ColRightPanel = ColorTranslator.FromHtml("#141A2E");
        private readonly Color ColCard = ColorTranslator.FromHtml("#1E2740");
        private readonly Color ColCardHover = ColorTranslator.FromHtml("#26304D");
        private readonly Color ColAccent = ColorTranslator.FromHtml("#2FE0E0");
        private readonly Color ColTextPrimary = Color.White;
        private readonly Color ColTextSecondary = ColorTranslator.FromHtml("#8A93A6");
        private readonly Color ColBadgeSU = ColorTranslator.FromHtml("#3DDC84");
        private readonly Color ColBadgeR13 = ColorTranslator.FromHtml("#E8635D");

        // ===== State =====
        private DataTable _films;
        private DataRow _selectedFilm;
        private readonly List<Panel> _cardPanels = new List<Panel>();
        private DataTable _jadwalTable;
        private DataRow _selectedJadwal;

        // ===== Controls =====
        private Panel pnlHeader;
        private Button btnBack;
        private Label lblTitle;
        private Button btnFilter;
        private ComboBox cmbGenre;
        private FlowLayoutPanel flpCards;
        private Panel pnlDetail;
        private PictureBox picDetailPoster;
        private Label lblDetailTitle;
        private FlowLayoutPanel flpDetailBadges;
        private Label lblDetailDesc;
        private Label lblGenreHeader;
        private Label lblGenreValue;
        private Label lblShowtimesHeader;
        private ComboBox cmbTanggal;
        private FlowLayoutPanel flpShowtimes;
        private Label lblSelectedInfo;
        private Button btnPilihKursi;

        public FormKatalog()
        {
            InitializeUi();
            this.Load += FormKatalog_Load;
        }

        // =========================================================
        //  UI SETUP (dibangun via code, tanpa Designer.cs terpisah)
        // =========================================================
        private void InitializeUi()
        {
            this.Text = "CineFlow - Select Your Movie";
            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ColBackground;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9F);

            // ---------- HEADER ----------
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColHeader
            };

            btnBack = new Button
            {
                Text = "←  BACK",
                FlatStyle = FlatStyle.Flat,
                ForeColor = ColTextPrimary,
                BackColor = ColHeader,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(90, 30),
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => this.Close();

            lblTitle = new Label
            {
                Text = "SELECT YOUR MOVIE",
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 16)
            };

            btnFilter = new Button
            {
                Text = "▤ Filter",
                FlatStyle = FlatStyle.Flat,
                ForeColor = ColTextPrimary,
                BackColor = ColHeader,
                Font = new Font("Segoe UI", 10F),
                Size = new Size(80, 32),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += (s, e) => { cmbGenre.Visible = !cmbGenre.Visible; };

            cmbGenre = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false,
                Size = new Size(160, 28),
                Font = new Font("Segoe UI", 9.5F)
            };
            cmbGenre.SelectedIndexChanged += (s, e) => LoadFilms(cmbGenre.SelectedItem?.ToString());

            pnlHeader.Controls.Add(btnBack);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnFilter);
            pnlHeader.Controls.Add(cmbGenre);
            pnlHeader.Resize += (s, e) => PositionHeaderControls();

            // ---------- CARD GRID (kiri) ----------
            flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBackground,
                AutoScroll = true,
                Padding = new Padding(20, 20, 10, 20)
            };

            var pnlLeftWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBackground
            };
            pnlLeftWrap.Controls.Add(flpCards);

            // ---------- DETAIL PANEL (kanan) ----------
            pnlDetail = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = ColRightPanel,
                Padding = new Padding(20),
                AutoScroll = true
            };
            BuildDetailPanel();

            var pnlBody = new Panel { Dock = DockStyle.Fill, BackColor = ColBackground };
            pnlBody.Controls.Add(pnlLeftWrap);
            pnlBody.Controls.Add(pnlDetail);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);

            this.Shown += (s, e) => PositionHeaderControls();
        }

        private void PositionHeaderControls()
        {
            lblTitle.Location = new Point((pnlHeader.Width - lblTitle.Width) / 2, 16);
            btnFilter.Location = new Point(pnlHeader.Width - 100, 14);
            cmbGenre.Location = new Point(pnlHeader.Width - 270, 16);
        }

        private void BuildDetailPanel()
        {
            int y = 0;

            picDetailPoster = new PictureBox
            {
                Size = new Size(120, 170),
                Location = new Point(0, y),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = ColCard
            };
            y += 180;

            lblDetailTitle = new Label
            {
                Text = "",
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Location = new Point(0, y),
                Size = new Size(260, 44)
            };
            y += 48;

            flpDetailBadges = new FlowLayoutPanel
            {
                Location = new Point(0, y),
                Size = new Size(260, 30),
                BackColor = Color.Transparent
            };
            y += 36;

            lblDetailDesc = new Label
            {
                Text = "",
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(0, y),
                Size = new Size(260, 90)
            };
            y += 98;

            lblGenreHeader = new Label
            {
                Text = "GENRE",
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 18;

            lblGenreValue = new Label
            {
                Text = "",
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(0, y),
                Size = new Size(260, 20)
            };
            y += 34;

            lblShowtimesHeader = new Label
            {
                Text = "SHOWTIMES",
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 20;

            cmbTanggal = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, y),
                Size = new Size(150, 26),
                Font = new Font("Segoe UI", 9F)
            };
            // Handler perubahan tanggal dipasang di LoadJadwal() lewat CmbTanggal_SelectedIndexChanged
            y += 34;

            flpShowtimes = new FlowLayoutPanel
            {
                Location = new Point(0, y),
                Size = new Size(260, 44),
                BackColor = Color.Transparent
            };
            y += 48;

            lblSelectedInfo = new Label
            {
                Text = "",
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(0, y),
                Size = new Size(260, 40)
            };
            y += 44;

            btnPilihKursi = new Button
            {
                Text = "PILIH KURSI  →",
                Size = new Size(260, 42),
                Location = new Point(0, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnPilihKursi.FlatAppearance.BorderSize = 0;
            btnPilihKursi.Click += BtnPilihKursi_Click;

            pnlDetail.Controls.Add(picDetailPoster);
            pnlDetail.Controls.Add(lblDetailTitle);
            pnlDetail.Controls.Add(flpDetailBadges);
            pnlDetail.Controls.Add(lblDetailDesc);
            pnlDetail.Controls.Add(lblGenreHeader);
            pnlDetail.Controls.Add(lblGenreValue);
            pnlDetail.Controls.Add(lblShowtimesHeader);
            pnlDetail.Controls.Add(cmbTanggal);
            pnlDetail.Controls.Add(flpShowtimes);
            pnlDetail.Controls.Add(lblSelectedInfo);
            pnlDetail.Controls.Add(btnPilihKursi);
        }

        // =========================================================
        //  DATA LOADING
        // =========================================================
        private void FormKatalog_Load(object sender, EventArgs e)
        {
            try
            {
                // 1. Ambil daftar genre dari database
                var genres = DatabaseHelper.GetDistinctGenres();

                cmbGenre.Items.Clear();
                cmbGenre.Items.Add("Semua Genre"); // Tambahkan opsi default

                if (genres != null && genres.Count > 0)
                {
                    cmbGenre.Items.AddRange(genres.ToArray());
                }

                // 2. Lepas sementara handler event agar tidak memicu LoadFilms secara tidak sengaja
                cmbGenre.SelectedIndexChanged -= CmbGenre_SelectedIndexChanged;
                cmbGenre.SelectedIndex = 0; // Pilih "Semua Genre"
                cmbGenre.SelectedIndexChanged += CmbGenre_SelectedIndexChanged;

                // 3. Muat seluruh film tanpa filter
                LoadFilms(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal memuat data dari database.\n\nDetail: " + ex.Message,
                    "Kesalahan Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbGenre_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = cmbGenre.SelectedItem?.ToString();
            // Jika memilih "Semua Genre", kirim null agar menampilkan seluruh film
            if (selected == "Semua Genre")
            {
                LoadFilms(null);
            }
            else
            {
                LoadFilms(selected);
            }
        }

        private void LoadFilms(string genreFilter)
        {
            _films = DatabaseHelper.GetFilms(genreFilter);
            BuildCards();

            if (_films.Rows.Count > 0)
                SelectFilm(_films.Rows[0]);
            else
                ClearDetail();
        }

        // =========================================================
        //  CARD GRID
        // =========================================================
        private void BuildCards()
        {
            flpCards.Controls.Clear();
            _cardPanels.Clear();

            foreach (DataRow row in _films.Rows)
            {
                var card = CreateMovieCard(row);
                flpCards.Controls.Add(card);
                _cardPanels.Add(card);
            }
        }

        private Panel CreateMovieCard(DataRow film)
        {
            var card = new Panel
            {
                Size = new Size(150, 250),
                Margin = new Padding(8),
                BackColor = ColCard,
                Cursor = Cursors.Hand,
                Tag = film
            };

            var pic = new PictureBox
            {
                Size = new Size(134, 180),
                Location = new Point(8, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = ColorTranslator.FromHtml("#0E1424"),
                Tag = film
            };
            LoadPosterImage(pic, film["Poster_Path"]?.ToString());

            var lblTitleCard = new Label
            {
                Text = film["Judul"].ToString(),
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(8, 194),
                Size = new Size(134, 32),
                Tag = film
            };

            string ratingText = film["Rating_Usia"] == DBNull.Value ? "SU" : film["Rating_Usia"].ToString();
            var lblRating = MakeBadge(ratingText, ratingText.StartsWith("SU") ? ColBadgeSU : ColBadgeR13);
            lblRating.Location = new Point(8, 228);
            lblRating.Tag = film;

            card.Controls.Add(pic);
            card.Controls.Add(lblTitleCard);
            card.Controls.Add(lblRating);

            // Klik di manapun pada card / child-nya akan memilih film
            EventHandler clickHandler = (s, e) =>
            {
                SelectFilm(film);
            };
            card.Click += clickHandler;
            pic.Click += clickHandler;
            lblTitleCard.Click += clickHandler;
            lblRating.Click += clickHandler;

            return card;
        }

        private Label MakeBadge(string text, Color backColor)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.Black,
                BackColor = backColor,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(6, 2, 6, 2),
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private void SelectFilm(DataRow film)
        {
            _selectedFilm = film;

            // Highlight card terpilih
            foreach (var card in _cardPanels)
            {
                var row = (DataRow)card.Tag;
                bool isSelected = row["Film_ID"].Equals(film["Film_ID"]);
                card.BackColor = isSelected ? ColCardHover : ColCard;
                DrawCardBorder(card, isSelected);
            }

            PopulateDetail(film);
        }

        private void DrawCardBorder(Panel card, bool selected)
        {
            // Gunakan Paint event untuk gambar border cyan saat dipilih
            card.Paint -= Card_Paint;
            if (selected)
            {
                card.Paint += Card_Paint;
            }
            card.Invalidate();
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var card = (Panel)sender;
            using (var pen = new Pen(ColAccent, 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, card.Width - 3, card.Height - 3);
            }
        }

        // =========================================================
        //  DETAIL PANEL
        // =========================================================
        private void PopulateDetail(DataRow film)
        {
            LoadPosterImage(picDetailPoster, film["Poster_Path"]?.ToString());

            lblDetailTitle.Text = film["Judul"].ToString();

            flpDetailBadges.Controls.Clear();
            string ratingText = film["Rating_Usia"] == DBNull.Value ? "SU" : film["Rating_Usia"].ToString();
            var badgeRating = MakeBadge(ratingText, ratingText.StartsWith("SU") ? ColBadgeSU : ColBadgeR13);
            badgeRating.Margin = new Padding(0, 0, 6, 0);
            flpDetailBadges.Controls.Add(badgeRating);

            int durasi = film["Durasi_Menit"] == DBNull.Value ? 0 : Convert.ToInt32(film["Durasi_Menit"]);
            var badgeDurasi = MakeBadge(durasi + "m", ColorTranslator.FromHtml("#3A4360"));
            badgeDurasi.ForeColor = ColTextPrimary;
            flpDetailBadges.Controls.Add(badgeDurasi);

            string sinopsis = film["Sinopsis"] == DBNull.Value ? "" : film["Sinopsis"].ToString();
            lblDetailDesc.Text = string.IsNullOrWhiteSpace(sinopsis)
                ? "Belum ada sinopsis untuk film ini."
                : sinopsis;

            lblGenreValue.Text = film["Genre"] == DBNull.Value ? "-" : film["Genre"].ToString();

            LoadJadwal(film);
        }

        // =========================================================
        //  JADWAL TAYANG (dari tabel jadwal_tayang)
        // =========================================================
        private void LoadJadwal(DataRow film)
        {
            _selectedJadwal = null;
            btnPilihKursi.Enabled = false;
            cmbTanggal.SelectedIndexChanged -= CmbTanggal_SelectedIndexChanged;
            cmbTanggal.SelectedIndexChanged += CmbTanggal_SelectedIndexChanged;

            int filmId = Convert.ToInt32(film["Film_ID"]);

            try
            {
                _jadwalTable = DatabaseHelper.GetJadwalByFilm(filmId);
            }
            catch (Exception ex)
            {
                _jadwalTable = null;
                flpShowtimes.Controls.Clear();
                cmbTanggal.Items.Clear();
                cmbTanggal.Visible = false;
                lblSelectedInfo.Text = "Gagal memuat jadwal: " + ex.Message;
                return;
            }

            cmbTanggal.Items.Clear();

            if (_jadwalTable == null || _jadwalTable.Rows.Count == 0)
            {
                cmbTanggal.Visible = false;
                flpShowtimes.Controls.Clear();
                lblSelectedInfo.Text = "Belum ada jadwal tayang untuk film ini.";
                return;
            }

            cmbTanggal.Visible = true;

            var distinctDates = _jadwalTable.AsEnumerable()
                .Select(r => Convert.ToDateTime(r["Tanggal_Tayang"]))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            foreach (var d in distinctDates)
            {
                cmbTanggal.Items.Add(d.ToString("dd MMM yyyy"));
            }

            cmbTanggal.SelectedIndex = 0; // otomatis memicu BuildShowtimesForSelectedDate
        }

        private void CmbTanggal_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildShowtimesForSelectedDate();
        }

        private void BuildShowtimesForSelectedDate()
        {
            flpShowtimes.Controls.Clear();
            lblSelectedInfo.Text = "";
            _selectedJadwal = null;
            btnPilihKursi.Enabled = false;

            if (_jadwalTable == null || cmbTanggal.SelectedIndex < 0)
                return;

            DateTime selectedDate = DateTime.ParseExact(
                cmbTanggal.SelectedItem.ToString(), "dd MMM yyyy",
                System.Globalization.CultureInfo.InvariantCulture);

            var rowsForDate = _jadwalTable.AsEnumerable()
                .Where(r => Convert.ToDateTime(r["Tanggal_Tayang"]).Date == selectedDate.Date)
                .OrderBy(r => r["Jam_Mulai"])
                .ToList();

            bool first = true;
            foreach (var row in rowsForDate)
            {
                string jamText = FormatJam(row["Jam_Mulai"]);

                var btn = new Button
                {
                    Text = jamText,
                    Size = new Size(76, 34),
                    Margin = new Padding(0, 0, 8, 8),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = ColCard,
                    ForeColor = ColTextPrimary,
                    Cursor = Cursors.Hand,
                    Tag = row
                };
                btn.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#3A4360");
                btn.FlatAppearance.BorderSize = 1;

                btn.Click += (s, e) =>
                {
                    _selectedJadwal = (DataRow)((Button)s).Tag;
                    foreach (Control c in flpShowtimes.Controls)
                    {
                        if (c is Button b)
                        {
                            bool sel = b == s;
                            b.BackColor = sel ? ColAccent : ColCard;
                            b.ForeColor = sel ? Color.Black : ColTextPrimary;
                        }
                    }
                    UpdateSelectedInfo();
                };

                flpShowtimes.Controls.Add(btn);

                if (first)
                {
                    btn.PerformClick();
                    first = false;
                }
            }
        }

        private void UpdateSelectedInfo()
        {
            if (_selectedJadwal == null)
            {
                lblSelectedInfo.Text = "";
                btnPilihKursi.Enabled = false;
                return;
            }

            string studio = _selectedJadwal["Nama_Studio"] == DBNull.Value
                ? "-" : _selectedJadwal["Nama_Studio"].ToString();

            decimal harga = _selectedJadwal["Harga_Tiket"] == DBNull.Value
                ? 0 : Convert.ToDecimal(_selectedJadwal["Harga_Tiket"]);

            lblSelectedInfo.Text = $"Studio: {studio}\nHarga Tiket: Rp {harga:N0}";
            btnPilihKursi.Enabled = true;
        }

        private void BtnPilihKursi_Click(object sender, EventArgs e)
        {
            if (_selectedFilm == null || _selectedJadwal == null) return;

            using (var formKursi = new FormPilihKursi(_selectedFilm, _selectedJadwal))
            {
                var result = formKursi.ShowDialog(this);

                if (result == DialogResult.OK && formKursi.HasilTransaksi != null)
                {
                    // Tampilkan struk/tiket
                    var kursiTable = DatabaseHelper.GetKursiByStudio(Convert.ToInt32(_selectedJadwal["Studio_ID"]));
                    using (var formTiket = new FormTiket(_selectedFilm, _selectedJadwal, formKursi.HasilTransaksi, kursiTable))
                    {
                        formTiket.ShowDialog(this);
                    }

                    // Refresh jadwal (kursi yang baru dipesan akan mengurangi ketersediaan)
                    LoadJadwal(_selectedFilm);
                }
            }
        }

        private static string FormatJam(object jamValue)
        {
            if (jamValue == null || jamValue == DBNull.Value) return "-";

            // Jam_Mulai bisa berupa TimeSpan (kolom TIME) atau DateTime tergantung driver
            if (jamValue is TimeSpan ts)
                return ts.ToString(@"hh\:mm");

            if (jamValue is DateTime dt)
                return dt.ToString("HH:mm");

            TimeSpan parsed;
            if (TimeSpan.TryParse(jamValue.ToString(), out parsed))
                return parsed.ToString(@"hh\:mm");

            return jamValue.ToString();
        }

        private void ClearDetail()
        {
            picDetailPoster.Image = null;
            lblDetailTitle.Text = "Tidak ada film";
            flpDetailBadges.Controls.Clear();
            lblDetailDesc.Text = "Belum ada film pada kategori ini.";
            lblGenreValue.Text = "-";
            cmbTanggal.Items.Clear();
            cmbTanggal.Visible = false;
            flpShowtimes.Controls.Clear();
            lblSelectedInfo.Text = "";
            btnPilihKursi.Enabled = false;
        }

        // =========================================================
        //  POSTER IMAGE LOADING (mendukung path lokal maupun URL)
        // =========================================================
        private void LoadPosterImage(PictureBox pic, string posterPath)
        {
            pic.Image = null;

            if (string.IsNullOrWhiteSpace(posterPath))
            {
                pic.BackColor = ColorTranslator.FromHtml("#0E1424");
                return;
            }

            try
            {
                if (posterPath.StartsWith("http://") || posterPath.StartsWith("https://"))
                {
                    using (var client = new WebClient())
                    {
                        byte[] data = client.DownloadData(posterPath);
                        using (var ms = new MemoryStream(data))
                        {
                            pic.Image = Image.FromStream(ms);
                        }
                    }
                }
                else if (File.Exists(posterPath))
                {
                    using (var fs = new FileStream(posterPath, FileMode.Open, FileAccess.Read))
                    {
                        pic.Image = Image.FromStream(fs);
                    }
                }
                else
                {
                    // Coba relatif terhadap folder aplikasi
                    string localPath = Path.Combine(Application.StartupPath, posterPath);
                    if (File.Exists(localPath))
                    {
                        using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                        {
                            pic.Image = Image.FromStream(fs);
                        }
                    }
                }
            }
            catch
            {
                // Gagal load poster -> biarkan kosong/placeholder
                pic.Image = null;
                pic.BackColor = ColorTranslator.FromHtml("#0E1424");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormKatalog
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "FormKatalog";
            this.Load += new System.EventHandler(this.FormKatalog_Load_1);
            this.ResumeLayout(false);

        }

        private void FormKatalog_Load_1(object sender, EventArgs e)
        {

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                // Buka Form Login
                using (login formlogin = new login())
                {
                    DialogResult result = formlogin.ShowDialog();

                    // Jika login berhasil
                    if (result == DialogResult.OK)
                    {
                        this.Hide();
                    }
                }

                return true; // Menandakan shortcut F5 sudah diproses penuh
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
