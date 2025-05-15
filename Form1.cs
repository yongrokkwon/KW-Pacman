using static KW_Pacman.Player;
namespace KW_Pacman
{
    public partial class Form1 : Form
    {
        private Bitmap[,] sprites; // [Direction, frame]
        private PointF spawnPos = new PointF(0, 0);
        private Direction spawnDir = Direction.Down;

        private Player player;

        private bool isGameover = false;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true; // 화면 깜박임 방지
            KeyPreview = true; // 키 입력 선처리 (키 입력을 전역적으로 처리 가능)

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

            timer1.Start();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
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

            // 디버그용 – 스페이스 누르면 강제 사망
            if (e.KeyCode == Keys.Space) player.Die();
        }

        // 죽으면 1초가 일시정지
        private void OnPlayerDied(object sender, EventArgs e)
        {
            if (isGameover) return;

            timer1.Stop();
            var t = new System.Windows.Forms.Timer { Interval = 1000 }; // 1초 정지를 위함
            t.Tick += (s2, _) =>
            {
                t.Stop();
                t.Dispose();

                if (player.lives > 0)
                {
                    player.Respawn();
                    timer1.Start();
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
            pacman_pb.Image = sprites[dirIdx, player.FrameIndex];

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            player.Update(timer1.Interval);      
            SyncView();                       
        }

    }
}
