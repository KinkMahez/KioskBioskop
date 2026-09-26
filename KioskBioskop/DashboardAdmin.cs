using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace KioskBioskop
{
    public partial class DashboardAdmin : Form
    {

        private string connString = "Server=localhost;Port=3306;Database=dbkiostiket;Uid=root;Pwd=;";
        public DashboardAdmin()
        {
            InitializeComponent();
        }

        private void DashboardAdmin_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            dashboard menu = new dashboard() { TopLevel = false, TopMost = true };
            panelControl.untukform(menu, panelContent);
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            film menu = new film() { TopLevel = false, TopMost = true };
            panelControl.untukform(menu, panelContent);
        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblJam_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblJam.Text = DateTime.Now.ToString("dddd : HH : mm");
        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            studio menu = new studio() { TopLevel = false, TopMost = true };
            panelControl.untukform(menu, panelContent);
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            jadwalFilm menu = new jadwalFilm() { TopLevel = false, TopMost = true };
            panelControl.untukform(menu, panelContent);
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DashboardAdmin_Load(object sender, EventArgs e)
        {
            dashboard menu = new dashboard() { TopLevel = false, TopMost = true };
            panelControl.untukform(menu, panelContent);
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            laporan menu = new laporan() { TopLevel = false, TopMost = true };
            panelControl.untukform(menu, panelContent);
        }
    }
}
