using KW_Pacman;
using static KW_Pacman.Player;

namespace PACKMAN
{
    public partial class Form1 : Form
    {
        // 가상의 미로 크기(추후 실제 맵과 동일하게 맞출 것)
        private const int MAZE_ROWS = 21;
        private const int MAZE_COLS = 19;
        private const int CELL_SIZE = 24; // 타일 한 칸 크기

        // 0 = 빈칸, 1 = 벽 (예시 미로, 실제 미로로 대체)
        private int[,] MazeGrid = new int[MAZE_ROWS, MAZE_COLS];

        // 유령, 팩맨의 위치를 "미로 좌표"로 관리 (그리드 단위)
        private Point ghostPos = new Point(1, 1);   // 예: (1,1)에서 시작
        private Point pacmanPos = new Point(10, 10);

        // 이동경로 (A*로 계산)
        private Queue<Point> ghostPath = new Queue<Point>();

        enum GhostState { Scatter, Chase, Frightened, Eaten, Respawning }

        private GhostState currentGhostState = GhostState.Scatter;
        private int stateTimer = 0;
        private int ghostMoveTimer = 0; // 유령 이동 타이밍 제어

        private Bitmap[,] sprites; // [Direction, frame]
        private PointF spawnPos = new PointF(240, 240); // 팩맨 시작 위치
        private Direction spawnDir = Direction.Right;

        private Player player;
        private bool isGameover = false;

        // 타이머 통합을 위한 카운터
        private int gameFrameCounter = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            // 화면 설정
            this.DoubleBuffered = true; // 화면 깜박임 방지
            this.KeyPreview = true; // 키 입력 선처리
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            // 스프라이트 로드
            LoadSprites();

            // Player 초기화
            player = new Player(spawnPos, spawnDir);
            player.Died += OnPlayerDied;

            // 타이머 설정 (하나로 통합)
            timer1.Interval = 50; // 20 FPS
            timer1.Tick += GameLoop_Tick;
            timer1.Start();

            // 초기 위치 설정
            ghostPos = new Point(1, 1);
            pacmanPos = PixelToMaze(Point.Round(player.Position));

            // PictureBox 설정
            if (pictureBox1 != null)
            {
                pictureBox1.Image = KW_Pacman.Properties.Resource.RedGhost;
                pictureBox1.Location = MazeToPixel(ghostPos);
            }
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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameover) return;

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
                case Keys.Space: // 디버그용 - 유령을 Frightened 상태로 변경
                    currentGhostState = GhostState.Frightened;
                    stateTimer = 0;
                    return;
                default:
                    dir = Direction.None;
                    break;
            }
            player.SetDirection(dir);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isGameover)
                player.SetStopped();
        }

        // 통합된 게임 루프
        private void GameLoop_Tick(object sender, EventArgs e)
        {
            if (isGameover) return;

            gameFrameCounter++;

            // 팩맨 업데이트 (매 프레임)
            player.Update(timer1.Interval);
            UpdatePacmanView();

            // 유령 업데이트 (2프레임마다 = 10 FPS)
            if (gameFrameCounter % 2 == 0)
            {
                UpdateGhost();
            }

            // 충돌 검사
            CheckCollisions();
        }

        private void UpdatePacmanView()
        {
            // 팩맨 위치 업데이트
            if (pictureBoxPacman != null)
            {
                pictureBoxPacman.Location = Point.Round(player.Position);
                
                // 스프라이트 업데이트
                int dirIdx = (int)player.Facing;
                if (dirIdx >= 0 && dirIdx < 4)
                {
                    pictureBoxPacman.Image = sprites[dirIdx, player.FrameIndex];
                }
            }

            // 팩맨의 미로 좌표 업데이트
            pacmanPos = PixelToMaze(Point.Round(player.Position));
        }

        private void UpdateGhost()
        {
            stateTimer++;
            ghostMoveTimer++;

            // 유령 상태 변경
            UpdateGhostState();

            // 유령 이동 (4프레임마다 = 5 FPS)
            if (ghostMoveTimer >= 4)
            {
                MoveGhost();
                ghostMoveTimer = 0;
            }

            // 유령 스프라이트 업데이트
            UpdateGhostSprite();
        }

        private void UpdateGhostState()
        {
            switch (currentGhostState)
            {
                case GhostState.Scatter:
                    if (stateTimer >= 60) // 3초 (60프레임)
                    {
                        currentGhostState = GhostState.Chase;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Chase:
                    if (stateTimer >= 60) // 3초
                    {
                        currentGhostState = GhostState.Frightened;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Frightened:
                    if (stateTimer >= 40) // 2초
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Eaten:
                    currentGhostState = GhostState.Respawning;
                    stateTimer = 0;
                    // 시작 위치로 이동
                    ghostPos = new Point(1, 1);
                    break;

                case GhostState.Respawning:
                    if (stateTimer >= 20) // 1초
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                    }
                    break;
            }
        }

        private void MoveGhost()
        {
            // 경로가 없거나 목표가 변경되었으면 새로 계산
            if (ghostPath.Count == 0)
            {
                Point targetPos = GetGhostTarget();
                ghostPath = FindPathAStar(ghostPos, targetPos);
            }

            // 유령 이동
            if (ghostPath.Count > 0)
            {
                ghostPos = ghostPath.Dequeue();
            }

            // 유령 위치 업데이트
            if (pictureBox1 != null)
            {
                pictureBox1.Location = MazeToPixel(ghostPos);
            }
        }

        private Point GetGhostTarget()
        {
            switch (currentGhostState)
            {
                case GhostState.Chase:
                    return pacmanPos; // 팩맨을 쫓기
                case GhostState.Scatter:
                    return new Point(0, 0); // 왼쪽 상단으로
                case GhostState.Frightened:
                    // 랜덤하게 이동
                    Random rand = new Random();
                    return new Point(rand.Next(MAZE_COLS), rand.Next(MAZE_ROWS));
                case GhostState.Respawning:
                    return new Point(1, 1); // 시작 위치로
                default:
                    return pacmanPos;
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
                case GhostState.Respawning:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.GhostEye;
                    pictureBox1.Visible = true;
                    break;
            }
        }

        private void CheckCollisions()
        {
            // 팩맨과 유령 충돌 검사
            if (pictureBox1 != null && pictureBoxPacman != null)
            {
                Rectangle ghostBounds = pictureBox1.Bounds;
                Rectangle pacmanBounds = pictureBoxPacman.Bounds;

                if (ghostBounds.IntersectsWith(pacmanBounds))
                {
                    if (currentGhostState == GhostState.Frightened)
                    {
                        // 유령을 먹음
                        currentGhostState = GhostState.Eaten;
                        stateTimer = 0;
                        ghostPath.Clear(); // 경로 초기화
                    }
                    else if (currentGhostState != GhostState.Eaten && 
                             currentGhostState != GhostState.Respawning)
                    {
                        // 팩맨이 죽음
                        player.Die();
                    }
                }
            }
        }

        private void OnPlayerDied(object sender, EventArgs e)
        {
            if (isGameover) return;

            timer1.Stop();
            var deathTimer = new System.Windows.Forms.Timer { Interval = 1000 }; // 1초 정지
            deathTimer.Tick += (s2, _) =>
            {
                deathTimer.Stop();
                deathTimer.Dispose();

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

            deathTimer.Start();
        }

        // --- A* 알고리즘 구현 ---
        private Queue<Point> FindPathAStar(Point start, Point goal)
        {
            var path = new Queue<Point>();

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

            while (openSet.Count > 0)
            {
                var current = openSet.Min.pos;
                if (current == goal)
                {
                    // 경로 복원
                    var totalPath = new List<Point> { current };
                    while (cameFrom.ContainsKey(current))
                    {
                        current = cameFrom[current];
                        totalPath.Add(current);
                    }
                    totalPath.Reverse();
                    // 시작 위치는 빼고 반환
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
            return path; // 경로 없으면 빈 큐 반환
        }

        private int Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private List<Point> GetNeighbors(Point p)
        {
            var dirs = new Point[] { 
                new Point(0, 1), new Point(1, 0), 
                new Point(0, -1), new Point(-1, 0) 
            }; // 상하좌우
            
            var neighbors = new List<Point>();
            foreach (var d in dirs)
            {
                int nx = p.X + d.X, ny = p.Y + d.Y;
                if (nx >= 0 && nx < MAZE_COLS && ny >= 0 && ny < MAZE_ROWS && 
                    MazeGrid[ny, nx] == 0)
                {
                    neighbors.Add(new Point(nx, ny));
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
    }
}