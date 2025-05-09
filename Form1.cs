using System;
using System.Windows.Forms;

namespace PACKMAN
{
    public partial class Form1 : Form
    {
        enum GhostState { Scatter, Chase, Frightened, Eaten, Respawning }

        private GhostState currentGhostState = GhostState.Scatter;
        private int stateTimer = 0;

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            timer2.Interval = 100;
            timer2.Start();

            pictureBox1.Image = Properties.Resources.RedGhost;
            pictureBoxPacman.BackColor = System.Drawing.Color.Yellow; // 임시 팩맨 색
            pictureBoxPacman.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                currentGhostState = GhostState.Frightened;
                stateTimer = 0;
            }

            // 팩맨 방향키 이동
            int step = 10;
            if (e.KeyCode == Keys.Left)
                pictureBoxPacman.Left -= step;
            else if (e.KeyCode == Keys.Right)
                pictureBoxPacman.Left += step;
            else if (e.KeyCode == Keys.Up)
                pictureBoxPacman.Top -= step;
            else if (e.KeyCode == Keys.Down)
                pictureBoxPacman.Top += step;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            stateTimer++;

            switch (currentGhostState)
            {
                case GhostState.Scatter:
                    pictureBox1.Image = Properties.Resources.RedGhost;
                    pictureBox1.Left -= 5;
                    if (stateTimer >= 30)
                    {
                        currentGhostState = GhostState.Chase;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Chase:
                    pictureBox1.Image = Properties.Resources.RedGhost;
                    pictureBox1.Left += 5;
                    if (stateTimer >= 30)
                    {
                        currentGhostState = GhostState.Frightened;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Frightened:
                    pictureBox1.Image = Properties.Resources.ScaredGhost;
                    pictureBox1.Left -= 2;
                    if (stateTimer >= 20)
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Eaten:
                    pictureBox1.Image = Properties.Resources.GhostEye;
                    pictureBox1.Visible = false;
                    currentGhostState = GhostState.Respawning;
                    stateTimer = 0;
                    break;

                case GhostState.Respawning:
                    pictureBox1.Image = Properties.Resources.GhostEye;
                    if (!pictureBox1.Visible)
                        pictureBox1.Visible = true;

                    if (pictureBox1.Left > 100)
                        pictureBox1.Left -= 5;
                    else if (pictureBox1.Left < 100)
                        pictureBox1.Left += 5;
                    else
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                    }
                    break;
            }

            // 팩맨과 충돌 감지
            if (pictureBox1.Bounds.IntersectsWith(pictureBoxPacman.Bounds))
            {
                if (currentGhostState == GhostState.Frightened)
                {
                    currentGhostState = GhostState.Eaten;
                    stateTimer = 0;
                }
                else if (currentGhostState != GhostState.Eaten && currentGhostState != GhostState.Respawning)
                {
                    timer2.Stop();
                    MessageBox.Show("팩맨이 잡혔습니다!");
                }
            }
        }
    }
}
