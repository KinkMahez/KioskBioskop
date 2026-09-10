using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KioskBioskop
{
    public class FormPilihKursi : Form
    {
        // ===== Palet warna (senada dengan FormKatalog) =====
        private readonly Color ColBackground = ColorTranslator.FromHtml("#12182B");
        private readonly Color ColHeader = ColorTranslator.FromHtml("#171E33");
        private readonly Color ColCard = ColorTranslator.FromHtml("#1E2740");
        private readonly Color ColAccent = ColorTranslator.FromHtml("#2FE0E0");
        private readonly Color ColBooked = ColorTranslator.FromHtml("#4A3140");
        private readonly Color ColBookedText = ColorTranslator.FromHtml("#E8635D");
        private readonly Color ColDisabled = ColorTranslator.FromHtml("#232A40");
        private readonly Color ColTextPrimary = Color.White;
        private readonly Color ColTextSecondary = ColorTranslator.FromHtml("#8A93A6");

        // ===== Input dari FormKatalog =====
        private readonly DataRow _film;
        private readonly DataRow _jadwal;
        private readonly int _jadwalId;
        private readonly int _studioId;
        private readonly decimal _hargaTiket;

        // ===== State =====
        private DataTable _kursiTable;
        private HashSet<int> _bookedIds;
        private readonly HashSet<int> _selectedIds = new HashSet<int>();
        private readonly Dictionary<int, Button> _seatButtons = new Dictionary<int, Button>();

        // ===== Hasil (dibaca oleh pemanggil setelah ShowDialog) =====
        public TransaksiResult HasilTransaksi { get; private set; }

        // ===== Controls =====
        private Label lblHeaderTitle;
        private Label lblHeaderInfo;
        private Panel pnlScreen;
        private Panel pnlSeatsScroll;
        private FlowLayoutPanel flpLegend;
        private Label lblSelectedSeats;
        private Label lblTotalHarga;
        private Button btnBayar;
        private Button btnBatal;

        public FormPilihKursi(DataRow film, DataRow jadwal)
        {
            _film = film;
            _jadwal = jadwal;
            _jadwalId = Convert.ToInt32(jadwal["Jadwal_ID"]);
            _studioId = Convert.ToInt32(jadwal["Studio_ID"]);
            _hargaTiket = jadwal["Harga_Tiket"] == DBNull.Value ? 0 : Convert.ToDecimal(jadwal["Harga_Tiket"]);

            InitializeUi();
            this.Load += FormPilihKursi_Load;
        }

        private void InitializeUi()
        {
            this.Text = "CineFlow - Pilih Kursi";
            this.Size = new Size(760, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ColBackground;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9F);

            // ---------- HEADER ----------
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = ColHeader };

            string namaStudio = _jadwal["Nama_Studio"] == DBNull.Value ? "-" : _jadwal["Nama_Studio"].ToString();
            string tanggal = Convert.ToDateTime(_jadwal["Tanggal_Tayang"]).ToString("dd MMM yyyy");
            string jam = FormatJam(_jadwal["Jam_Mulai"]);

            lblHeaderTitle = new Label
            {
                Text = _film["Judul"].ToString(),
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 10)
            };

            lblHeaderInfo = new Label
            {
                Text = $"{namaStudio}   •   {tanggal}   •   {jam}",
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = true,
                Location = new Point(24, 40)
            };

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderInfo);

            // ---------- SCREEN BAR ----------
            pnlScreen = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = ColBackground
            };
            pnlScreen.Paint += (s, e) =>
            {
                var rect = new Rectangle(60, 24, pnlScreen.Width - 120, 14);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    rect, ColorTranslator.FromHtml("#2FE0E0"), ColorTranslator.FromHtml("#12182B"),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                var textSize = e.Graphics.MeasureString("LAYAR", this.Font);
                e.Graphics.DrawString("LAYAR", new Font("Segoe UI", 8F, FontStyle.Bold), Brushes.Gray,
                    (pnlScreen.Width - textSize.Width) / 2, 2);
            };

            // ---------- SEAT GRID (scrollable) ----------
            pnlSeatsScroll = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBackground,
                AutoScroll = true,
                Padding = new Padding(20, 10, 20, 10)
            };

            // ---------- LEGEND ----------
            flpLegend = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = ColBackground,
                Padding = new Padding(20, 4, 20, 4)
            };
            flpLegend.Controls.Add(MakeLegendItem(ColCard, "Tersedia"));
            flpLegend.Controls.Add(MakeLegendItem(ColAccent, "Dipilih"));
            flpLegend.Controls.Add(MakeLegendItem(ColBooked, "Terisi"));
            flpLegend.Controls.Add(MakeLegendItem(ColDisabled, "Tidak Aktif"));

            // ---------- FOOTER ----------
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 90, BackColor = ColHeader };

            lblSelectedSeats = new Label
            {
                Text = "Belum ada kursi dipilih",
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 14)
            };

            lblTotalHarga = new Label
            {
                Text = "Total: Rp 0",
                ForeColor = ColAccent,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 38)
            };

            btnBatal = new Button
            {
                Text = "BATAL",
                Size = new Size(110, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColCard,
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatal.FlatAppearance.BorderSize = 0;
            btnBatal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            btnBayar = new Button
            {
                Text = "BAYAR SEKARANG",
                Size = new Size(190, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnBayar.FlatAppearance.BorderSize = 0;
            btnBayar.Click += BtnBayar_Click;

            pnlFooter.Resize += (s, e) =>
            {
                btnBayar.Location = new Point(pnlFooter.Width - btnBayar.Width - 24, 25);
                btnBatal.Location = new Point(pnlFooter.Width - btnBayar.Width - btnBatal.Width - 36, 25);
            };

            pnlFooter.Controls.Add(lblSelectedSeats);
            pnlFooter.Controls.Add(lblTotalHarga);
            pnlFooter.Controls.Add(btnBayar);
            pnlFooter.Controls.Add(btnBatal);

            this.Controls.Add(pnlSeatsScroll);
            this.Controls.Add(flpLegend);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlScreen);
            this.Controls.Add(pnlHeader);

            this.Shown += (s, e) =>
            {
                btnBayar.Location = new Point(pnlFooter.Width - btnBayar.Width - 24, 25);
                btnBatal.Location = new Point(pnlFooter.Width - btnBayar.Width - btnBatal.Width - 36, 25);
            };
        }

        private Panel MakeLegendItem(Color color, string text)
        {
            var wrap = new Panel { Size = new Size(110, 22), Margin = new Padding(0, 4, 10, 4) };
            var box = new Panel { Size = new Size(16, 16), Location = new Point(0, 3), BackColor = color };
            var lbl = new Label
            {
                Text = text,
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(22, 2),
                AutoSize = true
            };
            wrap.Controls.Add(box);
            wrap.Controls.Add(lbl);
            return wrap;
        }

        // =========================================================
        //  LOAD DATA & BANGUN PETA KURSI
        // =========================================================
        private void FormPilihKursi_Load(object sender, EventArgs e)
        {
            try
            {
                _kursiTable = DatabaseHelper.GetKursiByStudio(_studioId);
                _bookedIds = DatabaseHelper.GetBookedKursiIds(_jadwalId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data kursi.\n\nDetail: " + ex.Message,
                    "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_kursiTable.Rows.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "Belum ada data kursi untuk studio ini.",
                    ForeColor = ColTextSecondary,
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                pnlSeatsScroll.Controls.Add(lblEmpty);
                return;
            }

            BuildSeatMap();
        }

        private void BuildSeatMap()
        {
            pnlSeatsScroll.Controls.Clear();
            _seatButtons.Clear();

            var groupedByRow = _kursiTable.AsEnumerable()
                .GroupBy(r => r["Kode_Baris"].ToString())
                .OrderBy(g => g.Key)
                .ToList();

            const int seatSize = 34;
            const int seatGap = 6;
            const int rowGap = 10;
            const int rowLabelWidth = 30;

            int y = 10;

            foreach (var group in groupedByRow)
            {
                var seatsInRow = group
                    .OrderBy(r =>
                    {
                        int n;
                        return int.TryParse(r["Nomor_Kolom"].ToString(), out n) ? n : int.MaxValue;
                    })
                    .ThenBy(r => r["Nomor_Kolom"].ToString())
                    .ToList();

                var lblRow = new Label
                {
                    Text = group.Key,
                    ForeColor = ColTextSecondary,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Location = new Point(0, y + 6),
                    Size = new Size(rowLabelWidth, seatSize)
                };
                pnlSeatsScroll.Controls.Add(lblRow);

                int x = rowLabelWidth + 10;
                foreach (DataRow kursi in seatsInRow)
                {
                    var btn = CreateSeatButton(kursi, x, y, seatSize);
                    pnlSeatsScroll.Controls.Add(btn);
                    _seatButtons[Convert.ToInt32(kursi["Kursi_ID"])] = btn;
                    x += seatSize + seatGap;
                }

                y += seatSize + rowGap;
            }
        }

        private Button CreateSeatButton(DataRow kursi, int x, int y, int size)
        {
            int kursiId = Convert.ToInt32(kursi["Kursi_ID"]);
            string label = kursi["Label_Kursi"] == DBNull.Value
                ? kursi["Kode_Baris"].ToString() + kursi["Nomor_Kolom"].ToString()
                : kursi["Label_Kursi"].ToString();
            string statusFisik = kursi["Status_Fisik"] == DBNull.Value ? "ACTIVE" : kursi["Status_Fisik"].ToString();

            bool isBooked = _bookedIds.Contains(kursiId);
            bool isActive = string.Equals(statusFisik, "ACTIVE", StringComparison.OrdinalIgnoreCase);

            var btn = new Button
            {
                Text = kursi["Nomor_Kolom"].ToString(),
                Location = new Point(x, y),
                Size = new Size(size, size),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Tag = kursiId
            };
            btn.FlatAppearance.BorderSize = 1;

            var tip = new ToolTip();
            tip.SetToolTip(btn, $"Kursi {label}" + (isBooked ? " (sudah terisi)" : !isActive ? " (tidak aktif)" : ""));

            if (!isActive)
            {
                btn.BackColor = ColDisabled;
                btn.ForeColor = ColTextSecondary;
                btn.FlatAppearance.BorderColor = ColDisabled;
                btn.Enabled = false;
            }
            else if (isBooked)
            {
                btn.BackColor = ColBooked;
                btn.ForeColor = ColBookedText;
                btn.FlatAppearance.BorderColor = ColBooked;
                btn.Enabled = false;
            }
            else
            {
                btn.BackColor = ColCard;
                btn.ForeColor = ColTextPrimary;
                btn.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#3A4360");
                btn.Click += (s, e) => ToggleSeat(kursiId, btn);
            }

            return btn;
        }

        private void ToggleSeat(int kursiId, Button btn)
        {
            if (_selectedIds.Contains(kursiId))
            {
                _selectedIds.Remove(kursiId);
                btn.BackColor = ColCard;
                btn.ForeColor = ColTextPrimary;
                btn.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#3A4360");
            }
            else
            {
                _selectedIds.Add(kursiId);
                btn.BackColor = ColAccent;
                btn.ForeColor = Color.Black;
                btn.FlatAppearance.BorderColor = ColAccent;
            }

            UpdateFooter();
        }

        private void UpdateFooter()
        {
            if (_selectedIds.Count == 0)
            {
                lblSelectedSeats.Text = "Belum ada kursi dipilih";
                lblTotalHarga.Text = "Total: Rp 0";
                btnBayar.Enabled = false;
                return;
            }

            var labels = _selectedIds
                .Select(id => _kursiTable.AsEnumerable().First(r => Convert.ToInt32(r["Kursi_ID"]) == id))
                .Select(r => r["Label_Kursi"] == DBNull.Value
                    ? r["Kode_Baris"].ToString() + r["Nomor_Kolom"].ToString()
                    : r["Label_Kursi"].ToString())
                .OrderBy(l => l);

            lblSelectedSeats.Text = $"Kursi ({_selectedIds.Count}): " + string.Join(", ", labels);

            decimal total = _hargaTiket * _selectedIds.Count;
            lblTotalHarga.Text = $"Total: Rp {total:N0}";
            btnBayar.Enabled = true;
        }

        // =========================================================
        //  PROSES PEMBAYARAN / TRANSAKSI
        // =========================================================
        private void BtnBayar_Click(object sender, EventArgs e)
        {
            if (_selectedIds.Count == 0) return;

            var confirm = MessageBox.Show(
                $"Konfirmasi pembelian {_selectedIds.Count} tiket senilai Rp {(_hargaTiket * _selectedIds.Count):N0}?",
                "Konfirmasi Pembayaran", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            string kioskId = System.Configuration.ConfigurationManager.AppSettings["KioskId"];
            if (string.IsNullOrWhiteSpace(kioskId)) kioskId = "KIOSK-01";

            btnBayar.Enabled = false;
            btnBayar.Text = "MEMPROSES...";
            this.Cursor = Cursors.WaitCursor;

            try
            {
                HasilTransaksi = DatabaseHelper.CreateTransaksi(
                    kioskId, _jadwalId, _selectedIds.ToList(), _hargaTiket);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Transaksi gagal: " + ex.Message,
                    "Kesalahan Transaksi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Refresh peta kursi (kemungkinan ada kursi yang baru saja terisi oleh transaksi lain)
                try
                {
                    _bookedIds = DatabaseHelper.GetBookedKursiIds(_jadwalId);
                    _selectedIds.Clear();
                    BuildSeatMap();
                    UpdateFooter();
                }
                catch { /* abaikan error refresh sekunder */ }
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnBayar.Text = "BAYAR SEKARANG";
                btnBayar.Enabled = _selectedIds.Count > 0;
            }
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FormPilihKursi
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "FormPilihKursi";
            this.Load += new System.EventHandler(this.FormPilihKursi_Load_1);
            this.ResumeLayout(false);

        }

        private void FormPilihKursi_Load_1(object sender, EventArgs e)
        {

        }
    }
}
