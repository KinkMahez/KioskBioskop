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
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            DB.crud($"SELECT * FROM `users` WHERE Username = '{txtUser.Text}' and Password = MD5('{txtPw.Text}')");
            int cekbaris = DB.ds.Tables[0].Rows.Count;

            if (cekbaris == 1)
            {
                // 1. TAMBAHKAN BARIS INI (Wajib agar FormKatalog tahu login sukses)
                this.DialogResult = DialogResult.OK;

                // 2. Buka Dashboard Admin
                DashboardAdmin dashboard = new DashboardAdmin();
                dashboard.Show();

                // 3. Cari dan tutup/sembunyikan FormKatalog secara eksplisit
                Form formMenu = Application.OpenForms["FormKatalog"];
                if (formMenu != null)
                {
                    formMenu.Hide(); // atau formMenu.Close();
                }

                // 4. Tutup Form Login
                this.Close();
            }
            else
            {
                MessageBox.Show("Username atau password salah!", "Login Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
