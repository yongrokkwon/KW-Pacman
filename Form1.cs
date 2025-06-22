using KW_Pacman;
using static KW_Pacman.Player;

enum GhostState { Scatter, Chase, Frightened, Eaten, Respawning, ExitingHome }

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

        //private Point ghostPos = new Point(1, 1);        
        //private Queue<Point> ghostPath = new Queue<Point>();
        //private GhostState currentGhostState = GhostState.Scatter;
        //private int stateTimer = 0;
        //private int ghostMoveTimer = 0;

        private const int GHOST_COUNT = 3;
        private Point[] ghostPositions = new Point[GHOST_COUNT];
        private Queue<Point>[] ghostPaths = new Queue<Point>[GHOST_COUNT];
        private GhostState[] ghostStates = new GhostState[GHOST_COUNT];
        private int[] stateTimers = new int[GHOST_COUNT];
        private int[] ghostMoveTimers = new int[GHOST_COUNT];
        private PictureBox[] ghostPictureBoxes = new PictureBox[GHOST_COUNT];

        // 각 유령의 시작 위치
        private static readonly Point[] GHOST_START_POSITIONS = new Point[] {
            new Point(9, 10),   // 첫 번째 유령 - 유령집 중앙
            new Point(8, 10),   // 두 번째 유령 - 유령집 왼쪽
            new Point(10, 10)   // 세 번째 유령 - 유령집 오른쪽
        };

        // 각 유령의 색상/이미지
        private Image[] ghostImages = new Image[GHOST_COUNT];

        private Point pacmanPos = new Point(10, 10);
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
        // 유령의 시야 범위 (맨하탄 거리)
        private const int GHOST_SIGHT_RANGE = 5;

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

            // 유령 이미지 배열 초기화
            try
            {
                ghostImages[0] = KW_Pacman.Properties.Resource.RedGhost;
                ghostImages[1] = KW_Pacman.Properties.Resource.BlueGhost; // BlueGhost가 없으면 RedGhost 사용
                ghostImages[2] = KW_Pacman.Properties.Resource.PinkGhost; // PinkGhost가 없으면 RedGhost 사용
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ghost image loading failed: {ex.Message}");
                // 모든 유령을 빨간 유령으로 설정
                for (int i = 0; i < GHOST_COUNT; i++)
                {
                    ghostImages[i] = KW_Pacman.Properties.Resource.RedGhost;
                }
            }

            // 이미지가 없다면 모두 빨간 유령으로
            for (int i = 0; i < GHOST_COUNT; i++)
            {
                if (ghostImages[i] == null)
                    ghostImages[i] = KW_Pacman.Properties.Resource.RedGhost;
            }

            // 유령들 초기화
            for (int i = 0; i < GHOST_COUNT; i++)
            {
                ghostPositions[i] = GHOST_START_POSITIONS[i];
                ghostPaths[i] = new Queue<Point>();
                ghostStates[i] = GhostState.ExitingHome;
                stateTimers[i] = i * 20; // 각 유령이 다른 시점에 시작하도록 지연
                ghostMoveTimers[i] = 0;

                // PictureBox 생성 및 설정
                ghostPictureBoxes[i] = new PictureBox();
                ghostPictureBoxes[i].Image = ghostImages[i];
                ghostPictureBoxes[i].Location = MazeToPixel(ghostPositions[i]);
                //ghostPictureBoxes[i].BackColor = Color.Transparent;
                ghostPictureBoxes[i].Size = new Size(24, 24);
                ghostPictureBoxes[i].SizeMode = PictureBoxSizeMode.StretchImage; // 이 줄 추가!
                this.Controls.Add(ghostPictureBoxes[i]);
            }

            if (pictureBoxPacman != null)
            {
                pictureBoxPacman.Location = Point.Round(spawnPos);
                pictureBoxPacman.BackColor = Color.Transparent;
                pictureBoxPacman.Size = new Size(24, 24);
            }

            pacmanPos = PixelToMaze(Point.Round(player.Position));
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
                    MazeGrid[currentPos.Y, currentPos.X] = 0;
                    score += 50;
                    remainingDots--;
                    player.SetPowered();

                    // 모든 유령을 무서워하는 상태로 변경
                    for (int i = 0; i < GHOST_COUNT; i++)
                    {
                        if (ghostStates[i] != GhostState.Eaten && ghostStates[i] != GhostState.Respawning)
                        {
                            ghostStates[i] = GhostState.Frightened;
                            stateTimers[i] = 0;
                            ghostPaths[i].Clear();
                        }
                    }

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
            // 모든 유령에 대해 개별적으로 업데이트
            for (int ghostIndex = 0; ghostIndex < GHOST_COUNT; ghostIndex++)
            {
                UpdateSingleGhost(ghostIndex);
            }
        }

        private void UpdateSingleGhost(int ghostIndex)
        {
            stateTimers[ghostIndex]++;
            ghostMoveTimers[ghostIndex]++;

            UpdateGhostState(ghostIndex);

            // 각 유령마다 다른 속도 (약간의 변화를 위해)
            int moveInterval = 4 + (ghostIndex % 2); // 6, 7, 6 프레임마다
            if (ghostMoveTimers[ghostIndex] >= moveInterval)
            {
                MoveGhost(ghostIndex);
                ghostMoveTimers[ghostIndex] = 0;
            }

            UpdateGhostSprite(ghostIndex);
        }

        private void UpdateGhostState(int ghostIndex)
        {
            switch (ghostStates[ghostIndex])
            {
                case GhostState.ExitingHome:
                    if (ghostPositions[ghostIndex] == GHOST_SPAWN)
                    {
                        ghostStates[ghostIndex] = GhostState.Scatter;
                        stateTimers[ghostIndex] = 0;
                        Console.WriteLine($"Ghost {ghostIndex} exited home, starting Scatter mode");
                    }
                    break;

                case GhostState.Scatter:
                    if (IsValidPosition(pacmanPos) && GetManhattanDistance(ghostPositions[ghostIndex], pacmanPos) <= GHOST_SIGHT_RANGE)
                    {
                        ghostStates[ghostIndex] = GhostState.Chase;
                        stateTimers[ghostIndex] = 0;
                        ghostPaths[ghostIndex].Clear();
                        Console.WriteLine($"Ghost {ghostIndex} spotted Pacman! Switching to Chase mode");
                    }
                    break;

                case GhostState.Chase:
                    if (!IsValidPosition(pacmanPos) || GetManhattanDistance(ghostPositions[ghostIndex], pacmanPos) > GHOST_SIGHT_RANGE)
                    {
                        ghostStates[ghostIndex] = GhostState.Scatter;
                        stateTimers[ghostIndex] = 0;
                        ghostPaths[ghostIndex].Clear();
                        Console.WriteLine($"Ghost {ghostIndex} lost sight of Pacman, returning to Scatter");
                    }
                    else if (stateTimers[ghostIndex] >= 200)
                    {
                        ghostStates[ghostIndex] = GhostState.Scatter;
                        stateTimers[ghostIndex] = 0;
                        ghostPaths[ghostIndex].Clear();
                        Console.WriteLine($"Ghost {ghostIndex} chase timeout, returning to Scatter");
                    }
                    break;

                case GhostState.Frightened:
                    if (stateTimers[ghostIndex] >= 160)
                    {
                        ghostStates[ghostIndex] = GhostState.Scatter;
                        stateTimers[ghostIndex] = 0;
                        Console.WriteLine($"Ghost {ghostIndex} state: Scatter (from Frightened)");
                    }
                    break;

                case GhostState.Eaten:
                    break;

                case GhostState.Respawning:
                    if (stateTimers[ghostIndex] >= 20) // 1초 후 바로 나오기
                    {
                        ghostStates[ghostIndex] = GhostState.ExitingHome;
                        stateTimers[ghostIndex] = 0;
                        Console.WriteLine($"Ghost {ghostIndex} respawned and exiting home");
                    }
                    break;
            }
        }

        private void MoveGhost(int ghostIndex)
        {
            if (!IsValidPositionForGhost(ghostPositions[ghostIndex], ghostIndex))
            {
                Console.WriteLine($"ERROR: Ghost {ghostIndex} in wall at ({ghostPositions[ghostIndex].X}, {ghostPositions[ghostIndex].Y})!");
                ghostPositions[ghostIndex] = GHOST_SPAWN;
                ghostPaths[ghostIndex].Clear();
                return;
            }

            if (ghostStates[ghostIndex] == GhostState.Eaten)
            {
                if (ghostPositions[ghostIndex] == GHOST_START_POSITIONS[ghostIndex])
                {
                    ghostStates[ghostIndex] = GhostState.Respawning;
                    stateTimers[ghostIndex] = 0;
                    ghostPaths[ghostIndex].Clear();
                    Console.WriteLine($"Ghost {ghostIndex} arrived at home, respawning...");
                    return;
                }

                if (ghostPaths[ghostIndex].Count == 0)
                {
                    Console.WriteLine($"Ghost {ghostIndex} eaten! Finding path to home from ({ghostPositions[ghostIndex].X}, {ghostPositions[ghostIndex].Y})");
                    ghostPaths[ghostIndex] = FindPathAStar(ghostPositions[ghostIndex], GHOST_START_POSITIONS[ghostIndex], ghostIndex);

                    if (ghostPaths[ghostIndex].Count == 0)
                    {
                        ghostPositions[ghostIndex] = GHOST_START_POSITIONS[ghostIndex];
                        Console.WriteLine($"Direct teleport ghost {ghostIndex} to home");
                        return;
                    }
                }
            }
            else
            {
                if (ghostPaths[ghostIndex].Count == 0)
                {
                    Point targetPos = GetGhostTarget(ghostIndex);
                    ghostPaths[ghostIndex] = FindPathAStar(ghostPositions[ghostIndex], targetPos, ghostIndex);

                    if (ghostPaths[ghostIndex].Count == 0)
                    {
                        var neighbors = GetNeighbors(ghostPositions[ghostIndex], ghostIndex);
                        if (neighbors.Count > 0)
                        {
                            ghostPaths[ghostIndex].Enqueue(neighbors[0]);
                        }
                        else
                        {
                            Console.WriteLine($"Ghost {ghostIndex} completely stuck!");
                            return;
                        }
                    }
                }
            }

            if (ghostPaths[ghostIndex].Count > 0)
            {
                Point newPos = ghostPaths[ghostIndex].Dequeue();

                if (IsValidPositionForGhost(newPos, ghostIndex))
                {
                    ghostPositions[ghostIndex] = newPos;
                }
                else
                {
                    Console.WriteLine($"ERROR: Ghost {ghostIndex} blocked move to ({newPos.X}, {newPos.Y})");
                    ghostPaths[ghostIndex].Clear();
                    return;
                }
            }

            if (ghostPictureBoxes[ghostIndex] != null)
            {
                ghostPictureBoxes[ghostIndex].Location = MazeToPixel(ghostPositions[ghostIndex]);
            }
        }

        private bool IsValidPositionForGhost(Point pos, int ghostIndex)
        {
            if (pos.X < 0 || pos.X >= MAZE_COLS || pos.Y < 0 || pos.Y >= MAZE_ROWS)
                return false;

            int cellValue = MazeGrid[pos.Y, pos.X];

            // 벽 체크
            if (cellValue == 1) return false;

            // 다른 유령과의 충돌 체크 (같은 위치에 있으면 안됨)
            for (int i = 0; i < GHOST_COUNT; i++)
            {
                if (i != ghostIndex && ghostPositions[i] == pos)
                {
                    return false; // 다른 유령이 이미 있는 위치
                }
            }

            return true;
        }

        private Point GetGhostTarget(int ghostIndex)
        {
            switch (ghostStates[ghostIndex])
            {
                case GhostState.ExitingHome:
                    return GHOST_SPAWN;

                case GhostState.Chase:
                    if (IsValidPosition(pacmanPos) && GetManhattanDistance(ghostPositions[ghostIndex], pacmanPos) <= GHOST_SIGHT_RANGE)
                    {
                        return pacmanPos;
                    }
                    else
                    {
                        return GetRandomTarget(ghostIndex);
                    }

                case GhostState.Scatter:
                    return GetRandomTarget(ghostIndex);

                case GhostState.Frightened:
                    return GetFleeTarget(ghostIndex);

                case GhostState.Respawning:
                    return GHOST_START_POSITIONS[ghostIndex];

                default:
                    return GHOST_START_POSITIONS[ghostIndex];
            }
        }

        private Point GetRandomTarget(int ghostIndex)
        {
            Random rand = new Random(ghostIndex + DateTime.Now.Millisecond); // 각 유령마다 다른 시드

            var neighbors = GetNeighbors(ghostPositions[ghostIndex], ghostIndex);

            if (neighbors.Count > 0)
            {
                if (ghostPaths[ghostIndex].Count > 0 && rand.Next(100) < 70)
                {
                    return ghostPaths[ghostIndex].Peek();
                }

                Point randomDirection = neighbors[rand.Next(neighbors.Count)];
                int steps = rand.Next(2, 5);
                Point target = randomDirection;

                for (int i = 1; i < steps; i++)
                {
                    Point nextStep = new Point(
                        target.X + (randomDirection.X - ghostPositions[ghostIndex].X),
                        target.Y + (randomDirection.Y - ghostPositions[ghostIndex].Y)
                    );

                    if (IsValidPositionForGhost(nextStep, ghostIndex))
                        target = nextStep;
                    else
                        break;
                }

                return target;
            }

            return GHOST_START_POSITIONS[ghostIndex];
        }

        private Point GetFleeTarget(int ghostIndex)
        {
            Random rand = new Random(ghostIndex * 1000);
            Point bestTarget = ghostPositions[ghostIndex];
            int maxDistance = 0;

            for (int attempts = 0; attempts < 10; attempts++)
            {
                int x = rand.Next(1, MAZE_COLS - 1);
                int y = rand.Next(1, MAZE_ROWS - 1);
                Point candidate = new Point(x, y);

                if (MazeGrid[y, x] != 1)
                {
                    int distance = GetManhattanDistance(candidate, pacmanPos);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        bestTarget = candidate;
                    }
                }
            }

            return bestTarget;
        }

        private void UpdateGhostSprite(int ghostIndex)
        {
            if (ghostPictureBoxes[ghostIndex] == null) return;

            switch (ghostStates[ghostIndex])
            {
                case GhostState.ExitingHome:
                case GhostState.Scatter:
                case GhostState.Chase:
                    ghostPictureBoxes[ghostIndex].Image = ghostImages[ghostIndex];
                    ghostPictureBoxes[ghostIndex].Visible = true;
                    break;

                case GhostState.Frightened:
                    ghostPictureBoxes[ghostIndex].Image = KW_Pacman.Properties.Resource.ScaredGhost;
                    ghostPictureBoxes[ghostIndex].Visible = true;
                    break;

                case GhostState.Eaten:
                    ghostPictureBoxes[ghostIndex].Image = KW_Pacman.Properties.Resource.GhostEye;
                    ghostPictureBoxes[ghostIndex].Visible = true;
                    break;

                case GhostState.Respawning:
                    bool blink = (stateTimers[ghostIndex] / 10) % 2 == 0;
                    ghostPictureBoxes[ghostIndex].Image = ghostImages[ghostIndex];
                    ghostPictureBoxes[ghostIndex].Visible = blink;
                    break;
            }
        }

        private List<Point> GetNeighbors(Point p, int ghostIndex)
        {
            var dirs = new Point[] {
        new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0)
    };

            var neighbors = new List<Point>();
            foreach (var d in dirs)
            {
                int nx = p.X + d.X, ny = p.Y + d.Y;

                if (nx >= 0 && nx < MAZE_COLS && ny >= 0 && ny < MAZE_ROWS)
                {
                    int cellValue = MazeGrid[ny, nx];

                    if (ghostStates[ghostIndex] == GhostState.Eaten || ghostStates[ghostIndex] == GhostState.Respawning)
                    {
                        if (cellValue != 1)
                        {
                            neighbors.Add(new Point(nx, ny));
                        }
                    }
                    else
                    {
                        if (cellValue != 1)
                        {
                            // 다른 유령과의 충돌 체크
                            bool occupied = false;
                            for (int i = 0; i < GHOST_COUNT; i++)
                            {
                                if (i != ghostIndex && ghostPositions[i].X == nx && ghostPositions[i].Y == ny)
                                {
                                    occupied = true;
                                    break;
                                }
                            }
                            if (!occupied)
                                neighbors.Add(new Point(nx, ny));
                        }
                    }
                }
            }

            return neighbors;
        }

        private Queue<Point> FindPathAStar(Point start, Point goal, int ghostIndex)
        {
            var path = new Queue<Point>();

            if (!IsValidPositionForGhost(start, ghostIndex) || !IsValidPositionForGhost(goal, ghostIndex))
            {
                Console.WriteLine($"Invalid positions for ghost {ghostIndex} - Start: ({start.X}, {start.Y}), Goal: ({goal.X}, {goal.Y})");
                return path;
            }

            // 기존 A* 알고리즘과 동일하지만 GetNeighbors 호출 시 ghostIndex 전달
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
                foreach (var neighbor in GetNeighbors(current, ghostIndex))
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
                Console.WriteLine($"A* timeout for ghost {ghostIndex}!");

            return path;
        }

        // 맨하탄 거리 계산
        private int GetManhattanDistance(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private void CheckCollisions()
        {
            if (pictureBoxPacman == null) return;

            Rectangle pacmanBounds = pictureBoxPacman.Bounds;

            // 모든 유령과 충돌 체크
            for (int i = 0; i < GHOST_COUNT; i++)
            {
                if (ghostPictureBoxes[i] != null)
                {
                    Rectangle ghostBounds = ghostPictureBoxes[i].Bounds;

                    if (ghostBounds.IntersectsWith(pacmanBounds))
                    {
                        if (player.IsPowered() &&
                            ghostStates[i] != GhostState.Eaten &&
                            ghostStates[i] != GhostState.Respawning)
                        {
                            // 유령을 먹음
                            ghostStates[i] = GhostState.Eaten;
                            stateTimers[i] = 0;
                            ghostPaths[i].Clear();
                            score += 200;
                            UpdateScore();
                            Console.WriteLine($"Ghost {i} eaten by powered Pacman! Ghost returning home...");
                        }
                        else if (!player.IsPowered() &&
                                 ghostStates[i] != GhostState.Eaten &&
                                 ghostStates[i] != GhostState.Respawning)
                        {
                            player.Die();
                            Console.WriteLine($"Pacman caught by ghost {i}!");
                            return; // 하나의 유령에게만 잡히도록
                        }
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

            // 유령 초기화 (기존 단일 유령 코드 삭제하고 배열 버전으로 교체)
            for (int i = 0; i < GHOST_COUNT; i++)
            {
                ghostPositions[i] = GHOST_START_POSITIONS[i];
                ghostPaths[i].Clear();
                ghostStates[i] = GhostState.ExitingHome;
                stateTimers[i] = i * 20;
                ghostMoveTimers[i] = 0;
            }

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

            // 플레이어 이름 입력 받기
            string playerName = GetPlayerName();

            // 점수 저장
            ScoreManager.SaveScore(playerName, score);

            // 게임오버 다이얼로그 표시
            DialogResult result = MessageBox.Show(
                $"게임 오버!\n\n플레이어: {playerName}\n최종 점수: {score}\n\n다시 플레이 하시겠습니까?",
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
                // 폼 닫기 (메인메뉴로 돌아가기)
                this.Close();
            }
        }

        // 플레이어 이름 입력 받는 메서드 추가
        private string GetPlayerName()
        {
            string playerName = Microsoft.VisualBasic.Interaction.InputBox(
                "당신의 이름을 입력하세요:",
                "점수 기록",
                "Player"
            );

            // 이름이 비어있거나 취소했을 경우 기본값 사용
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Anonymous";
            }

            // 이름 길이 제한 (20자)
            if (playerName.Length > 20)
            {
                playerName = playerName.Substring(0, 20);
            }

            return playerName;
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
            spawnPos = new PointF(9 * CELL_SIZE, 1 * CELL_SIZE);
            player.ResetToPosition(spawnPos, spawnDir);

            // 모든 유령 리셋
            for (int i = 0; i < GHOST_COUNT; i++)
            {
                ghostPositions[i] = GHOST_START_POSITIONS[i];
                ghostPaths[i].Clear();
                ghostStates[i] = GhostState.ExitingHome;
                stateTimers[i] = i * 20; // 각 유령이 다른 시점에 시작
                ghostMoveTimers[i] = 0;

                if (ghostPictureBoxes[i] != null)
                {
                    ghostPictureBoxes[i].Location = MazeToPixel(ghostPositions[i]);
                }
            }

            if (pictureBoxPacman != null)
            {
                pictureBoxPacman.Location = Point.Round(spawnPos);
            }

            pacmanPos = PixelToMaze(Point.Round(spawnPos));
            Console.WriteLine("Positions reset - Pacman and all ghosts back to start");
        }

        private int Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
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
            if (isGameover) return;

            if (player == null)
            {
                Console.WriteLine("Player is null!");
                return;
            }

            if (player.State == PlayerState.Dead)
                return;

            // SetNormal() 호출 제거 또는 조건부로 변경
            if (player.State == PlayerState.Stopped)
            {
                player.SetNormal(); // 정지 상태일 때만 Normal로 변경
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