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
    public partial class Settings_window : Form
    {
        int amount_cards = 1;
        int amount_players = 1;

        public Settings_window()
        {
            InitializeComponent();
            btnReturn.Click += new EventHandler(BtnReturn_Click);
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public int Get_deck()
        {
            return amount_cards;
        }

        public int Get_players()
        {
            return amount_players;
        }
    }
}
