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
    public partial class Game_window : Form
    {
        int player_turn = 0;
        Game engine;
        public Game_window()
        {
            engine = new Game(2, 1);
            InitializeComponent();
            btnHit.Click += new EventHandler(BtnHit_Click);
            this.Hide();
        }

        public void BtnHit_Click(object sender, EventArgs e)
        {
            
        }
    }
}
