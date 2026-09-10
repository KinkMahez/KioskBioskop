using KiosBioskop;
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
    public partial class Welcome : Form
    {
        public Welcome()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            FormKatalog formSelanjutnya = new FormKatalog();
            formSelanjutnya.Show();

            this.Hide();
        }

        private void Welcome_Load(object sender, EventArgs e)
        {
            label1.Parent = guna2PictureBox1;
            labelJam.Parent = guna2PictureBox1; // ganti pictureBox1 sesuai nama PictureBox kamu

            // 2. Set BackColor ke Transparent
            label1.BackColor = Color.Transparent;
            labelJam.BackColor = Color.Transparent;


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelJam.Text = DateTime.Now.ToString("HH:mm");
        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void labelJam_Click(object sender, EventArgs e)
        {

        }
    }
}
