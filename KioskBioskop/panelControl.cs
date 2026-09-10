using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KioskBioskop
{
    internal class panelControl
    {
        public static void untukform(Form Fapa, Panel PnlApa)
        {
            PnlApa.Controls.Clear();
            PnlApa.Controls.Add(Fapa);
            Fapa.FormBorderStyle = FormBorderStyle.None;
            Fapa.Dock = DockStyle.Fill;
            Fapa.Show();
        }
    }
}
