using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KW_Pacman
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Form1 game = new Form1();
            DialogResult dResult = game.ShowDialog();
        }

        private void btnScoreBoard_Click(object sender, EventArgs e)
        {
            ScoreBoard sb = new ScoreBoard();
            DialogResult dResult = sb.ShowDialog();
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {

        }
    }
}
