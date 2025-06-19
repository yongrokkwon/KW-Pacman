using KW_Pacman;
using static KW_Pacman.Player;

namespace PACKMAN
{
    public partial class Form1 : Form
    {
        // 유령집 위치 정의
        private static readonly Point GHOST_HOME = new Point(9, 10); // 유령집 중앙
        private static readonly Point GHOST_SPAWN = new Point(9, 9);  // 유령집 입구

        // 미로 크기를 21x19로 수정
        private const int MAZE_ROWS = 21;  // 19 → 21로 수정
        private const int MAZE_COLS = 19;
        private const int CELL_SIZE = 24;

        // 0 = 빈칸, 1 = 벽
        public int[,] MazeGrid = new int[MAZE_ROWS, MAZE_COLS];

        // 유령, 팩맨의 위치를 "미로 좌표"로 관리 (그리드 단위)
        private Point ghostPos = new Point(1, 1);
        private Point pacmanPos = new Point(10, 10);

        // 이동경로 (A*로 계산)
        private Queue<Point> ghostPath = new Queue<Point>();

        enum GhostState { Scatter, Chase, Frightened, Eaten, Respawning }

        private GhostState currentGhostState = GhostState.Scatter;
        private int stateTimer = 0;
        private int ghostMoveTimer = 0;

        private Bitmap[,] sprites; // [Direction, frame]
        private PointF spawnPos = new PointF(240, 240);
        private Direction spawnDir = Direction.Right;

        private Player player;
        private bool isGameover = false;

        // 점수 관련 필드
        private int score = 0;
        private int totalDots = 0;
        private int remainingDots = 0;
        private Label scoreLabel;
        private Label livesLabel; // 목숨 표시 라벨 추가

        // 실제 이미지와 정확히 일치하는 미로 데이터
        private void InitializeSimpleMazeGrid()
        {
            // 미로 크기: 19x21 (가로 19칸, 세로 21칸)
            // 0 = 빈 공간, 1 = 벽, 2 = 터널 입구, 3 = 닷(점수), 4 = 파워펠렛
            int[,] simpleMaze = new int[21, 19] {
                // Row 0 (y=0) - 상단 외벽
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},

                // Row 1 (y=24) - 닷이 있는 복도
                {1,4,3,3,3,3,3,3,3,0,3,3,3,3,3,3,3,4,1},

                // Row 2 (y=48) - 상단 내부 벽들
                {1,3,1,1,1,3,1,1,1,1,1,1,1,3,1,1,1,3,1},

                // Row 3 (y=72) - 닷이 있는 복도
                {1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1},

                // Row 4 (y=96) - 2줄 벽들 (가로)
                {1,3,1,3,1,1,3,1,3,1,3,1,3,1,1,3,1,3,1},

                // Row 5 (y=120) - 2줄 벽들 (세로 연장)
                {1,3,1,3,3,3,3,1,3,1,3,1,3,3,3,3,1,3,1},

                // Row 6 (y=144) - 닷이 있는 복도
                {1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1},

                // Row 7 (y=168) - 3줄 가로 벽들
                {1,3,3,3,1,1,3,1,1,1,1,1,3,1,1,3,3,3,1},

                // Row 8 (y=192) - 닷이 있는 복도
                {1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1},

                // Row 9 (y=216) - 유령집 상단 + 좌우 박스 상단
                {1,3,1,1,1,3,3,1,1,0,1,1,3,3,1,1,1,3,1},

                // Row 10 (y=240) - 터널 + 유령집 + 좌우 박스 중간
                {2,3,1,1,1,3,3,1,0,0,0,1,3,3,1,1,1,3,2},

                // Row 11 (y=264) - 유령집 하단 + 좌우 박스 하단
                {1,3,1,1,1,3,3,1,1,1,1,1,3,3,1,1,1,3,1},

                // Row 12 (y=288) - 닷이 있는 복도
                {1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1},

                // Row 13 (y=312) - 닷이 있는 복도
                {1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1},

                // Row 14 (y=336) - 중하 가로 벽들
                {1,3,3,3,1,1,3,1,1,1,1,1,3,1,1,3,3,3,1},

                // Row 15 (y=360) - 닷이 있는 복도
                {1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1},

                // Row 16 (y=384) - 하단 벽들 (가로)
                {1,3,1,3,1,1,3,1,3,1,3,1,3,1,1,3,1,3,1},

                // Row 17 (y=408) - 하단 벽들 (세로 연장)
                {1,3,1,3,3,3,3,1,3,1,3,1,3,3,3,3,1,3,1},

                // Row 18 (y=432) - 닷이 있는 복도
                {1,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1},

                // Row 19 (y=456) - 최하단 내부 벽들
                {1,4,1,1,1,3,1,1,1,1,1,1,1,3,1,1,1,4,1},

                // Row 20 (y=480) - 하단 외벽
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            // MazeGrid에 복사
            for (int row = 0; row < MAZE_ROWS; row++)
            {
                for (int col = 0; col < MAZE_COLS; col++)
                {
                    MazeGrid[row, col] = simpleMaze[row, col];
                }
            }
        }

        public static Form1 Instance;

        public Form1()
        {
            Instance = this; // 생성자에 추가

            InitializeComponent();
            InitializeSimpleMazeGrid();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // 화면 설정
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            // Form 크기 설정 (21x19 미로에 맞게 조정)
            this.ClientSize = new Size(456, 504); // 19 * 24 = 456, 21 * 24 = 504
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.Black;

            // 스프라이트 로드
            LoadSprites();

            // Player 초기화 - 확실히 통로인 위치로 설정
            spawnPos = new PointF(9 * CELL_SIZE, 1 * CELL_SIZE); // (9,1)은 빈 공간
            
            try
            {
                player = new Player(spawnPos, spawnDir);
                player.Died += OnPlayerDied;
                Console.WriteLine("Player initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Player initialization failed: {ex.Message}");
                return;
            }

            // 점수 라벨 생성
            scoreLabel = new Label();
            scoreLabel.Text = "Score: 0";
            scoreLabel.ForeColor = Color.Yellow;
            scoreLabel.BackColor = Color.Black;
            scoreLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            scoreLabel.Location = new Point(10, 10);
            scoreLabel.Size = new Size(120, 25);
            this.Controls.Add(scoreLabel);

            // 목숨 라벨 생성
            livesLabel = new Label();
            livesLabel.Text = "Lives: 3";
            livesLabel.ForeColor = Color.Yellow;
            livesLabel.BackColor = Color.Black;
            livesLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            livesLabel.Location = new Point(150, 10); // 점수 라벨 옆에 위치
            livesLabel.Size = new Size(100, 25);
            this.Controls.Add(livesLabel);

            // 총 닷 개수 계산
            CountTotalDots();

            // 초기 목숨 표시 업데이트
            UpdateLives();

            // 타이머 설정
            timer1.Interval = 50;
            timer1.Tick += GameLoop_Tick;
            timer1.Start();

            // 초기 위치 설정 - 확실히 통로인 곳으로
            ghostPos = new Point(9, 9); // 유령집 근처
            pacmanPos = PixelToMaze(Point.Round(player.Position));

            // 위치 디버그 출력
            Console.WriteLine($"Ghost starting position: ({ghostPos.X}, {ghostPos.Y})");
            Console.WriteLine($"Pacman starting position: ({pacmanPos.X}, {pacmanPos.Y})");

            // 배열 경계 체크 후 출력
            if (ghostPos.Y >= 0 && ghostPos.Y < MAZE_ROWS && ghostPos.X >= 0 && ghostPos.X < MAZE_COLS)
                Console.WriteLine($"Ghost maze value: {MazeGrid[ghostPos.Y, ghostPos.X]}");
            else
                Console.WriteLine("Ghost position out of bounds!");

            if (pacmanPos.Y >= 0 && pacmanPos.Y < MAZE_ROWS && pacmanPos.X >= 0 && pacmanPos.X < MAZE_COLS)
                Console.WriteLine($"Pacman maze value: {MazeGrid[pacmanPos.Y, pacmanPos.X]}");
            else
                Console.WriteLine("Pacman position out of bounds!");

            // PictureBox 설정
            if (pictureBox1 != null)
            {
                pictureBox1.Image = KW_Pacman.Properties.Resource.RedGhost;
                pictureBox1.Location = MazeToPixel(ghostPos);
                pictureBox1.BackColor = Color.Transparent;
                pictureBox1.Size = new Size(24, 24);
            }

            if (pictureBoxPacman != null)
            {
                pictureBoxPacman.Location = Point.Round(spawnPos);
                pictureBoxPacman.BackColor = Color.Transparent;
                pictureBoxPacman.Size = new Size(24, 24);
            }
        }

        private void CountTotalDots()
        {
            totalDots = 0;
            for (int row = 0; row < MAZE_ROWS; row++)
            {
                for (int col = 0; col < MAZE_COLS; col++)
                {
                    if (MazeGrid[row, col] == 3 || MazeGrid[row, col] == 4)
                    {
                        totalDots++;
                    }
                }
            }
            remainingDots = totalDots;
        }

        private void LoadSprites()
        {
            sprites = new Bitmap[4, 3];

            sprites[(int)Direction.Left, 0] = KW_Pacman.Properties.Resources.pacman_left_0;
            sprites[(int)Direction.Left, 1] = KW_Pacman.Properties.Resources.pacman_left_1;
            sprites[(int)Direction.Left, 2] = KW_Pacman.Properties.Resources.pacman_left_2;

            sprites[(int)Direction.Right, 0] = KW_Pacman.Properties.Resources.pacman_right_0;
            sprites[(int)Direction.Right, 1] = KW_Pacman.Properties.Resources.pacman_right_1;
            sprites[(int)Direction.Right, 2] = KW_Pacman.Properties.Resources.pacman_right_2;

            sprites[(int)Direction.Up, 0] = KW_Pacman.Properties.Resources.pacman_up_0;
            sprites[(int)Direction.Up, 1] = KW_Pacman.Properties.Resources.pacman_up_1;
            sprites[(int)Direction.Up, 2] = KW_Pacman.Properties.Resources.pacman_up_2;

            sprites[(int)Direction.Down, 0] = KW_Pacman.Properties.Resources.pacman_down_0;
            sprites[(int)Direction.Down, 1] = KW_Pacman.Properties.Resources.pacman_down_1;
            sprites[(int)Direction.Down, 2] = KW_Pacman.Properties.Resources.pacman_down_2;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //if (!isGameover)
            //    player.SetStopped();
        }

        private void GameLoop_Tick(object sender, EventArgs e)
        {
            if (isGameover) return;

            player.Update(timer1.Interval);
            UpdatePacmanView();

            // 닷/파워펠렛 충돌 검사 추가
            CheckDotCollection();

            UpdateGhost();
            CheckCollisions();

            // 화면 새로고침
            this.Invalidate();
        }

        private void CheckDotCollection()
        {
            Point currentPos = PixelToMaze(Point.Round(player.Position));

            if (currentPos.X >= 0 && currentPos.X < MAZE_COLS &&
                currentPos.Y >= 0 && currentPos.Y < MAZE_ROWS)
            {
                int cellValue = MazeGrid[currentPos.Y, currentPos.X];

                if (cellValue == 3) // 닷
                {
                    MazeGrid[currentPos.Y, currentPos.X] = 0; // 먹음
                    score += 10;
                    remainingDots--;
                    UpdateScore();
                }
                else if (cellValue == 4) // 파워펠렛
                {
                    MazeGrid[currentPos.Y, currentPos.X] = 0; // 먹음
                    score += 50;
                    remainingDots--;
                    player.SetPowered(); // 플레이어를 파워 상태로

                    // 유령을 무서워하는 상태로 변경
                    currentGhostState = GhostState.Frightened;
                    stateTimer = 0;
                    ghostPath.Clear();

                    UpdateScore();
                }

                // 모든 닷을 먹으면 게임 클리어
                if (remainingDots <= 0)
                {
                    timer1.Stop();
                    MessageBox.Show($"Stage Clear! Final Score: {score}");
                    isGameover = true;
                }
            }
        }

        private void UpdateScore()
        {
            if (scoreLabel != null)
            {
                scoreLabel.Text = $"Score: {score}";
            }
        }

        private void UpdateLives()
        {
            if (livesLabel != null && player != null)
            {
                livesLabel.Text = $"Lives: {player.lives}";
            }
        }

        private bool IsValidPosition(Point pos)
        {
            return pos.X >= 0 && pos.X < MAZE_COLS &&
                   pos.Y >= 0 && pos.Y < MAZE_ROWS &&
                   (MazeGrid[pos.Y, pos.X] == 0 || MazeGrid[pos.Y, pos.X] == 3 || MazeGrid[pos.Y, pos.X] == 4);
        }

        private void UpdatePacmanView()
        {
            if (player == null || pictureBoxPacman == null) return;

            pictureBoxPacman.Location = Point.Round(player.Position);

            int dirIdx = (int)player.Facing;
            if (dirIdx >= 0 && dirIdx < 4)
            {
                pictureBoxPacman.Image = sprites[dirIdx, player.FrameIndex];
            }

            pacmanPos = PixelToMaze(Point.Round(player.Position));
        }

        private void UpdateGhost()
        {
            stateTimer++;
            ghostMoveTimer++;

            UpdateGhostState();

            // 유령 이동을 더 자주 하도록 수정 (3프레임마다)
            if (ghostMoveTimer >= 3)
            {
                MoveGhost();
                ghostMoveTimer = 0;
            }

            UpdateGhostSprite();
        }

        private void UpdateGhostState()
        {
            switch (currentGhostState)
            {
                case GhostState.Scatter:
                    if (stateTimer >= 100)
                    {
                        currentGhostState = GhostState.Chase;
                        stateTimer = 0;
                        Console.WriteLine("Ghost state: Chase");
                    }
                    break;

                case GhostState.Chase:
                    if (stateTimer >= 150)
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                        Console.WriteLine("Ghost state: Scatter");
                    }
                    break;

                case GhostState.Frightened:
                    if (stateTimer >= 160) // 8초 (160 * 50ms)
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                        Console.WriteLine("Ghost state: Scatter (from Frightened)");
                    }
                    break;

                case GhostState.Eaten:
                    // Eaten 상태에서는 즉시 유령집으로 향함
                    // 이동은 MoveGhost에서 처리
                    break;

                case GhostState.Respawning:
                    if (stateTimer >= 60) // 3초 후 정상 상태로
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                        Console.WriteLine("Ghost respawned and ready");
                    }
                    break;
            }
        }

        private void MoveGhost()
        {
            // 현재 위치가 유효한지 먼저 체크
            if (!IsValidPositionForGhost(ghostPos))
            {
                Console.WriteLine($"ERROR: Ghost in wall at ({ghostPos.X}, {ghostPos.Y})!");
                ghostPos = GHOST_SPAWN; // 유령집 입구로 이동
                ghostPath.Clear();
                return;
            }

            // Eaten 상태일 때는 유령집으로 직접 이동
            if (currentGhostState == GhostState.Eaten)
            {
                // 유령집에 도착했는지 확인
                if (ghostPos == GHOST_HOME)
                {
                    currentGhostState = GhostState.Respawning;
                    stateTimer = 0;
                    ghostPath.Clear();
                    Console.WriteLine("Ghost arrived at home, respawning...");
                    return;
                }

                // 유령집으로 가는 경로가 없으면 새로 계산
                if (ghostPath.Count == 0)
                {
                    Console.WriteLine($"Ghost eaten! Finding path to home from ({ghostPos.X}, {ghostPos.Y})");
                    ghostPath = FindPathAStar(ghostPos, GHOST_HOME);

                    if (ghostPath.Count == 0)
                    {
                        // 경로를 찾을 수 없으면 직접 이동
                        ghostPos = GHOST_HOME;
                        Console.WriteLine("Direct teleport to ghost home");
                        return;
                    }
                }
            }
            else
            {
                // 일반 상태에서의 이동 (기존 로직)
                if (ghostPath.Count == 0)
                {
                    Point targetPos = GetGhostTarget();
                    Console.WriteLine($"Finding path from ({ghostPos.X}, {ghostPos.Y}) to ({targetPos.X}, {targetPos.Y})");
                    ghostPath = FindPathAStar(ghostPos, targetPos);

                    if (ghostPath.Count == 0)
                    {
                        Console.WriteLine("No path found! Trying neighbors...");
                        var neighbors = GetNeighbors(ghostPos);
                        if (neighbors.Count > 0)
                        {
                            ghostPath.Enqueue(neighbors[0]);
                            Console.WriteLine($"Moving to neighbor: ({neighbors[0].X}, {neighbors[0].Y})");
                        }
                        else
                        {
                            Console.WriteLine($"Ghost completely stuck at ({ghostPos.X}, {ghostPos.Y})!");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Path found with {ghostPath.Count} steps");
                    }
                }
            }

            // 유령 이동
            if (ghostPath.Count > 0)
            {
                Point newPos = ghostPath.Dequeue();

                if (IsValidPositionForGhost(newPos))
                {
                    ghostPos = newPos;
                    Console.WriteLine($"Ghost moved to ({ghostPos.X}, {ghostPos.Y})");
                }
                else
                {
                    Console.WriteLine($"ERROR: Blocked move to ({newPos.X}, {newPos.Y})");
                    ghostPath.Clear();
                    return;
                }
            }

            // 유령 위치 업데이트
            if (pictureBox1 != null)
            {
                pictureBox1.Location = MazeToPixel(ghostPos);
            }
        }

        private bool IsValidPositionForGhost(Point pos)
        {
            if (pos.X < 0 || pos.X >= MAZE_COLS || pos.Y < 0 || pos.Y >= MAZE_ROWS)
                return false;

            int cellValue = MazeGrid[pos.Y, pos.X];

            // 유령은 벽(1)을 제외한 모든 곳을 지날 수 있음
            // Eaten 상태일 때는 유령집 내부(0)도 지날 수 있음
            if (currentGhostState == GhostState.Eaten || currentGhostState == GhostState.Respawning)
            {
                return cellValue != 1; // 벽만 막음
            }
            else
            {
                return cellValue != 1; // 일반 상태에서도 벽만 막음
            }
        }

        private Point GetGhostTarget()
        {
            switch (currentGhostState)
            {
                case GhostState.Chase:
                    if (IsValidPosition(pacmanPos))
                        return pacmanPos;
                    else
                        return new Point(9, 9); // 안전한 위치로 변경

                case GhostState.Scatter:
                    return new Point(1, 3); // 왼쪽 상단의 통로

                case GhostState.Frightened:
                    Random rand = new Random();
                    for (int attempts = 0; attempts < 20; attempts++)
                    {
                        int x = rand.Next(1, MAZE_COLS - 1);
                        int y = rand.Next(1, MAZE_ROWS - 1);
                        if (MazeGrid[y, x] != 1) // 벽이 아니면 OK
                            return new Point(x, y);
                    }
                    return new Point(9, 9);

                case GhostState.Respawning:
                    return new Point(9, 9);

                default:
                    return new Point(9, 9);
            }
        }

        private void UpdateGhostSprite()
        {
            if (pictureBox1 == null) return;

            switch (currentGhostState)
            {
                case GhostState.Scatter:
                case GhostState.Chase:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.RedGhost;
                    pictureBox1.Visible = true;
                    break;

                case GhostState.Frightened:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.ScaredGhost;
                    pictureBox1.Visible = true;
                    break;

                case GhostState.Eaten:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.GhostEye; // 눈알만 표시
                    pictureBox1.Visible = true;
                    break;

                case GhostState.Respawning:
                    // 깜빡이는 효과를 위해 타이머에 따라 표시/숨김
                    bool blink = (stateTimer / 10) % 2 == 0;
                    pictureBox1.Image = KW_Pacman.Properties.Resource.RedGhost;
                    pictureBox1.Visible = blink;
                    break;
            }
        }

        private void CheckCollisions()
        {
            if (pictureBox1 != null && pictureBoxPacman != null)
            {
                Rectangle ghostBounds = pictureBox1.Bounds;
                Rectangle pacmanBounds = pictureBoxPacman.Bounds;

                if (ghostBounds.IntersectsWith(pacmanBounds))
                {
                    if (currentGhostState == GhostState.Frightened && player.IsPowered())
                    {
                        // 유령을 먹음
                        currentGhostState = GhostState.Eaten;
                        stateTimer = 0;
                        ghostPath.Clear();
                        score += 200; // 유령을 먹으면 보너스 점수
                        UpdateScore();
                        Console.WriteLine("Ghost eaten by powered Pacman! Ghost returning home...");
                    }
                    else if (currentGhostState != GhostState.Eaten &&
                             currentGhostState != GhostState.Respawning)
                    {
                        player.Die();
                        Console.WriteLine("Pacman caught by ghost!");
                    }
                }
            }
        }

        private void RestartGame()
        {
            // 게임 상태 초기화
            isGameover = false;
            score = 0;

            // 미로 데이터 다시 초기화 (점수펠렛 복원)
            InitializeSimpleMazeGrid();
            CountTotalDots();

            // 플레이어 완전 초기화
            spawnPos = new PointF(9 * CELL_SIZE, 1 * CELL_SIZE);
            player.ResetToPosition(spawnPos, spawnDir);
            player.lives = 3; // 목숨 3개로 초기화

            // 유령 초기화
            ghostPos = new Point(9, 9);
            ghostPath.Clear();
            currentGhostState = GhostState.Scatter;
            stateTimer = 0;
            ghostMoveTimer = 0;

            // UI 업데이트
            UpdateScore();
            UpdateLives();

            // 위치 초기화
            ResetPositions();

            // 타이머 재시작
            timer1.Start();

            Console.WriteLine("Game restarted!");
        }

        private void GameOver()
        {
            isGameover = true;
            timer1.Stop();

            // 게임오버 다이얼로그 표시
            DialogResult result = MessageBox.Show(
                $"게임 오버!\n\n최종 점수: {score}\n\n다시 플레이 하시겠습니까?",
                "Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // 게임 재시작
                RestartGame();
            }
            else
            {
                // 애플리케이션 종료
                Application.Exit();
            }
        }

        private void OnPlayerDied(object sender, EventArgs e)
        {
            if (isGameover) return;

            timer1.Stop();

            // 목숨 표시 즉시 업데이트
            UpdateLives();

            // 1초 후에 목숨 체크
            var deathTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            deathTimer.Tick += (s2, _) =>
            {
                deathTimer.Stop();
                deathTimer.Dispose();

                // 목숨이 0이 되면 게임오버
                if (player.lives <= 0)
                {
                    GameOver();
                }
                else
                {
                    // 목숨이 남아있으면 리스폰
                    ResetPositions();
                    timer1.Start();
                }
            };

            deathTimer.Start();
        }

        private void ResetPositions()
        {
            // 팩맨 초기 위치로 리셋
            spawnPos = new PointF(9 * CELL_SIZE, 1 * CELL_SIZE);
            player.ResetToPosition(spawnPos, spawnDir);

            // 유령 초기 위치로 리셋
            ghostPos = new Point(9, 9);
            ghostPath.Clear();
            currentGhostState = GhostState.Scatter;
            stateTimer = 0;
            ghostMoveTimer = 0;

            // PictureBox 위치 업데이트
            if (pictureBox1 != null)
            {
                pictureBox1.Location = MazeToPixel(ghostPos);
            }

            if (pictureBoxPacman != null)
            {
                pictureBoxPacman.Location = Point.Round(spawnPos);
            }

            // 팩맨 위치 업데이트
            pacmanPos = PixelToMaze(Point.Round(spawnPos));

            Console.WriteLine("Positions reset - Pacman and Ghost back to start");
        }

        // A* 알고리즘
        private Queue<Point> FindPathAStar(Point start, Point goal)
        {
            var path = new Queue<Point>();

            if (!IsValidPosition(start) || !IsValidPosition(goal))
            {
                Console.WriteLine($"Invalid positions - Start: ({start.X}, {start.Y}), Goal: ({goal.X}, {goal.Y})");
                return path;
            }

            var openSet = new SortedSet<(int f, Point pos)>(
                Comparer<(int f, Point pos)>.Create(
                    (a, b) => a.f == b.f ? a.pos.GetHashCode().CompareTo(b.pos.GetHashCode()) : a.f.CompareTo(b.f)
                )
            );
            var cameFrom = new Dictionary<Point, Point>();
            var gScore = new Dictionary<Point, int>();
            var fScore = new Dictionary<Point, int>();

            openSet.Add((0, start));
            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);

            int iterations = 0;
            while (openSet.Count > 0 && iterations < 1000)
            {
                iterations++;
                var current = openSet.Min.pos;
                if (current == goal)
                {
                    var totalPath = new List<Point> { current };
                    while (cameFrom.ContainsKey(current))
                    {
                        current = cameFrom[current];
                        totalPath.Add(current);
                    }
                    totalPath.Reverse();
                    for (int i = 1; i < totalPath.Count; i++)
                        path.Enqueue(totalPath[i]);

                    return path;
                }

                openSet.Remove(openSet.Min);
                foreach (var neighbor in GetNeighbors(current))
                {
                    int tentativeG = gScore[current] + 1;
                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                        if (!openSet.Contains((fScore[neighbor], neighbor)))
                            openSet.Add((fScore[neighbor], neighbor));
                    }
                }
            }

            if (iterations >= 1000)
                Console.WriteLine("A* timeout!");

            return path;
        }

        private int Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private List<Point> GetNeighbors(Point p)
        {
            var dirs = new Point[] {
        new Point(0, 1),  // 아래
        new Point(1, 0),  // 오른쪽
        new Point(0, -1), // 위
        new Point(-1, 0)  // 왼쪽
    };

            var neighbors = new List<Point>();
            foreach (var d in dirs)
            {
                int nx = p.X + d.X, ny = p.Y + d.Y;

                if (nx >= 0 && nx < MAZE_COLS && ny >= 0 && ny < MAZE_ROWS)
                {
                    int cellValue = MazeGrid[ny, nx];

                    // 유령 상태에 따른 이동 가능 여부 판단
                    if (currentGhostState == GhostState.Eaten || currentGhostState == GhostState.Respawning)
                    {
                        // 먹힌 상태에서는 벽만 막음 (유령집 통과 가능)
                        if (cellValue != 1)
                        {
                            neighbors.Add(new Point(nx, ny));
                        }
                    }
                    else
                    {
                        // 일반 상태에서는 벽만 막음
                        if (cellValue != 1)
                        {
                            neighbors.Add(new Point(nx, ny));
                        }
                    }
                }
            }

            return neighbors;
        }

        private Point MazeToPixel(Point mazePos)
        {
            return new Point(mazePos.X * CELL_SIZE, mazePos.Y * CELL_SIZE);
        }

        private Point PixelToMaze(Point pixelPos)
        {
            return new Point(pixelPos.X / CELL_SIZE, pixelPos.Y / CELL_SIZE);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameover) return; // 게임오버 시 키 입력 무시

            // player가 null인지 확인
            if (player == null)
            {
                Console.WriteLine("Player is null!");
                return;
            }

            // 플레이어가 죽은 상태면 키 입력 무시
            if (player.State == PlayerState.Dead)
                return;

            player.SetNormal();
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
            player.SetNextDirection(dir);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawMaze(e.Graphics);
        }

        private void DrawMaze(Graphics g)
        {
            for (int row = 0; row < MAZE_ROWS; row++)
            {
                for (int col = 0; col < MAZE_COLS; col++)
                {
                    int x = col * CELL_SIZE;
                    int y = row * CELL_SIZE;

                    switch (MazeGrid[row, col])
                    {
                        case 1: // 벽
                            g.FillRectangle(Brushes.Blue, x, y, CELL_SIZE, CELL_SIZE);
                            break;
                        case 3: // 닷
                            g.FillEllipse(Brushes.Yellow, x + 10, y + 10, 4, 4);
                            break;
                        case 4: // 파워펠렛
                            g.FillEllipse(Brushes.Yellow, x + 6, y + 6, 12, 12);
                            break;
                    }
                }
            }
        }
    }
}