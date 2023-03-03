using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blackjack
{
    public partial class Start_window : Form
    {
        public Start_window()
        {
            InitializeComponent();
            btnSettings.Click += new EventHandler(BtnSettings_Click);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            Settings_window settings = new Settings_window();
            settings.Show();
        }
    }
}
