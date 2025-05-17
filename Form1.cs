using System;
using System.Windows.Forms;
using static PACKMAN.Player;

namespace PACKMAN
{
    public partial class Form1 : Form
    {
        enum GhostState { Scatter, Chase, Frightened, Eaten, Respawning }

        private GhostState currentGhostState = GhostState.Scatter;
        private int stateTimer = 0;

        /* Plyaer */
        private Bitmap[,] sprites; // [Direction, frame]
        private PointF spawnPos = new PointF(0, 0);
        private Direction spawnDir = Direction.Down;
        private Player player;
        private bool isGameover = false;


        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            timer2.Interval = 100;

            pictureBox1.Image = Properties.Resources.RedGhost;
            //pictureBoxPacman.BackColor = System.Drawing.Color.Yellow; // 임시 팩맨 색
            pictureBoxPacman.SizeMode = PictureBoxSizeMode.AutoSize;

            /* 스프라이트 로드 */
            sprites = new Bitmap[4, 3];

            sprites[(int)Direction.Left, 0] = Properties.Resources.pacman_left_0;
            sprites[(int)Direction.Left, 1] = Properties.Resources.pacman_left_1;
            sprites[(int)Direction.Left, 2] = Properties.Resources.pacman_left_2;

            sprites[(int)Direction.Right, 0] = Properties.Resources.pacman_right_0;
            sprites[(int)Direction.Right, 1] = Properties.Resources.pacman_right_1;
            sprites[(int)Direction.Right, 2] = Properties.Resources.pacman_right_2;

            sprites[(int)Direction.Up, 0] = Properties.Resources.pacman_up_0;
            sprites[(int)Direction.Up, 1] = Properties.Resources.pacman_up_1;
            sprites[(int)Direction.Up, 2] = Properties.Resources.pacman_up_2;

            sprites[(int)Direction.Down, 0] = Properties.Resources.pacman_down_0;
            sprites[(int)Direction.Down, 1] = Properties.Resources.pacman_down_1;
            sprites[(int)Direction.Down, 2] = Properties.Resources.pacman_down_2;

            /* Player */
            player = new Player(spawnPos, spawnDir);
            player.Died += OnPlayerDied;

            timer2.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                currentGhostState = GhostState.Frightened;
                stateTimer = 0;
            }

            Direction dir;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    dir = Direction.Left;
                    break;
                case Keys.Right:
                    dir = Direction.Right;
                    break;
                case Keys.Up:
                    dir = Direction.Up;
                    break;
                case Keys.Down:
                    dir = Direction.Down;
                    break;
                default:
                    dir = Direction.None;
                    break;
            }
            player.SetDirection(dir);

            // 디버그용 – V 누르면 강제 사망
            if (e.KeyCode == Keys.V) player.Die();
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

            /* Player 업데이트 */
            player.Update(timer1.Interval);
            SyncView();
        }

        // 팩맨이 죽으면 1초 간 일시정지
        private void OnPlayerDied(object sender, EventArgs e)
        {
            if (isGameover) return;

            timer2.Stop();
            var t = new System.Windows.Forms.Timer { Interval = 1000 }; // 1초 정지를 위함
            t.Tick += (s2, _) =>
            {
                t.Stop();
                t.Dispose();

                if (player.lives > 0)
                {
                    player.Respawn();
                    timer2.Start();
                }
                else
                {
                    isGameover = true;
                    MessageBox.Show("Game Over");
                }
            };

            t.Start();
        }

        private void SyncView()
        {
            int dirIdx = (int)player.Facing;
            pictureBox1.Image = sprites[dirIdx, player.FrameIndex];

        }
    }
}
