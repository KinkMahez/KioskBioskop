using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KioskBioskop
{
    public class FormTiket : Form
    {
        private readonly Color ColBackground = ColorTranslator.FromHtml("#12182B");
        private readonly Color ColHeader = ColorTranslator.FromHtml("#171E33");
        private readonly Color ColCard = ColorTranslator.FromHtml("#1E2740");
        private readonly Color ColAccent = ColorTranslator.FromHtml("#2FE0E0");
        private readonly Color ColSuccess = ColorTranslator.FromHtml("#3DDC84");
        private readonly Color ColTextPrimary = Color.White;
        private readonly Color ColTextSecondary = ColorTranslator.FromHtml("#8A93A6");

        private readonly DataRow _film;
        private readonly DataRow _jadwal;
        private readonly TransaksiResult _hasil;
        private readonly DataTable _kursiTable;

        public FormTiket(DataRow film, DataRow jadwal, TransaksiResult hasil, DataTable kursiTable)
        {
            _film = film;
            _jadwal = jadwal;
            _hasil = hasil;
            _kursiTable = kursiTable;

            BuildUi();
        }

        private void BuildUi()
        {
            this.Text = "CineFlow - Tiket Anda";
            this.Size = new Size(460, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = ColBackground;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9F);

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = ColHeader };
            var lblCheck = new Label
            {
                Text = "✔  PEMBAYARAN BERHASIL",
                ForeColor = ColSuccess,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 20)
            };
            var lblKode = new Label
            {
                Text = "Kode Transaksi: " + _hasil.KodeTransaksi,
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = true,
                Location = new Point(24, 54)
            };
            pnlHeader.Controls.Add(lblCheck);
            pnlHeader.Controls.Add(lblKode);

            var pnlScroll = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColBackground,
                AutoScroll = true,
                Padding = new Padding(24, 16, 24, 16)
            };

            int y = 0;

            var lblJudul = new Label
            {
                Text = _film["Judul"].ToString(),
                ForeColor = ColTextPrimary,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(0, y),
                Size = new Size(400, 30)
            };
            y += 34;

            string namaStudio = _jadwal["Nama_Studio"] == DBNull.Value ? "-" : _jadwal["Nama_Studio"].ToString();
            string tanggal = Convert.ToDateTime(_jadwal["Tanggal_Tayang"]).ToString("dddd, dd MMM yyyy");
            string jam = FormatJam(_jadwal["Jam_Mulai"]);

            var lblInfo = new Label
            {
                Text = $"{namaStudio}\n{tanggal}  •  {jam}",
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(0, y),
                Size = new Size(400, 40)
            };
            y += 50;

            var sep1 = new Panel { Location = new Point(0, y), Size = new Size(400, 1), BackColor = ColCard };
            y += 16;

            var lblTiketHeader = new Label
            {
                Text = $"DAFTAR TIKET ({_hasil.TotalTiket})",
                ForeColor = ColTextSecondary,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };
            y += 24;

            pnlScroll.Controls.Add(lblJudul);
            pnlScroll.Controls.Add(lblInfo);
            pnlScroll.Controls.Add(sep1);
            pnlScroll.Controls.Add(lblTiketHeader);

            foreach (var tiket in _hasil.Tikets)
            {
                var kursiRow = _kursiTable.AsEnumerable()
                    .FirstOrDefault(r => Convert.ToInt32(r["Kursi_ID"]) == tiket.KursiId);

                string labelKursi = kursiRow == null
                    ? "Kursi #" + tiket.KursiId
                    : (kursiRow["Label_Kursi"] == DBNull.Value
                        ? kursiRow["Kode_Baris"].ToString() + kursiRow["Nomor_Kolom"].ToString()
                        : kursiRow["Label_Kursi"].ToString());

                var card = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(400, 66),
                    BackColor = ColCard
                };

                var lblSeat = new Label
                {
                    Text = "Kursi " + labelKursi,
                    ForeColor = ColTextPrimary,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    Location = new Point(14, 10),
                    AutoSize = true
                };

                var lblQr = new Label
                {
                    Text = tiket.KodeTiketQR,
                    ForeColor = ColAccent,
                    Font = new Font("Consolas", 8.5F),
                    Location = new Point(14, 36),
                    AutoSize = true
                };

                card.Controls.Add(lblSeat);
                card.Controls.Add(lblQr);
                pnlScroll.Controls.Add(card);

                y += 76;
            }

            var sep2 = new Panel { Location = new Point(0, y), Size = new Size(400, 1), BackColor = ColCard };
            y += 16;

            var lblTotal = new Label
            {
                Text = $"Total Bayar: Rp {_hasil.TotalBayar:N0}",
                ForeColor = ColAccent,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Location = new Point(0, y),
                AutoSize = true
            };

            pnlScroll.Controls.Add(sep2);
            pnlScroll.Controls.Add(lblTotal);

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = ColHeader };
            var btnSelesai = new Button
            {
                Text = "SELESAI",
                Size = new Size(160, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColAccent,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSelesai.FlatAppearance.BorderSize = 0;
            btnSelesai.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };
            pnlFooter.Resize += (s, e) =>
                btnSelesai.Location = new Point((pnlFooter.Width - btnSelesai.Width) / 2, 15);
            pnlFooter.Controls.Add(btnSelesai);

            this.Controls.Add(pnlScroll);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);

            this.Shown += (s, e) =>
                btnSelesai.Location = new Point((pnlFooter.Width - btnSelesai.Width) / 2, 15);
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
            // FormTiket
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "FormTiket";
            this.Load += new System.EventHandler(this.FormTiket_Load);
            this.ResumeLayout(false);

        }

        private void FormTiket_Load(object sender, EventArgs e)
        {

        }
    }
}
