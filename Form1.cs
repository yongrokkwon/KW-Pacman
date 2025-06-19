using KW_Pacman;
using static KW_Pacman.Player;

namespace PACKMAN
{
    public partial class Form1 : Form
    {
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

        // 타이머 통합을 위한 카운터
        private int gameFrameCounter = 0;

        // 실제 이미지와 정확히 일치하는 미로 데이터
        private void InitializeSimpleMazeGrid()
        {
            // 미로 크기: 19x21 (가로 19칸, 세로 21칸)
            // 0 = 빈 공간, 1 = 벽, 2 = 터널 입구
            int[,] simpleMaze = new int[21, 19] {
                // Row 0 (y=0) - 상단 외벽
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
        
                // Row 1 (y=24) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 2 (y=48) - 상단 내부 벽들
                {1,0,1,1,1,0,1,1,1,1,1,1,1,0,1,1,1,0,1},
        
                // Row 3 (y=72) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 4 (y=96) - 2줄 벽들 (가로)
                {1,0,1,0,1,1,0,1,0,1,0,1,0,1,1,0,1,0,1},
        
                // Row 5 (y=120) - 2줄 벽들 (세로 연장)
                {1,0,1,0,0,0,0,1,0,1,0,1,0,0,0,0,1,0,1},
        
                // Row 6 (y=144) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 7 (y=168) - 3줄 가로 벽들
                {1,0,0,0,1,1,0,1,1,1,1,1,0,1,1,0,0,0,1},
        
                // Row 8 (y=192) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 9 (y=216) - 유령집 상단 + 좌우 박스 상단
                {1,0,1,1,1,0,0,1,1,0,1,1,0,0,1,1,1,0,1},
        
                // Row 10 (y=240) - 터널 + 유령집 + 좌우 박스 중간
                {2,0,1,1,1,0,0,1,0,0,0,1,0,0,1,1,1,0,2},
        
                // Row 11 (y=264) - 유령집 하단 + 좌우 박스 하단
                {1,0,1,1,1,0,0,1,1,1,1,1,0,0,1,1,1,0,1},
        
                // Row 12 (y=288) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 13 (y=312) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 14 (y=336) - 중하 가로 벽들
                {1,0,0,0,1,1,0,1,1,1,1,1,0,1,1,0,0,0,1},
        
                // Row 15 (y=360) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 16 (y=384) - 하단 벽들 (가로)
                {1,0,1,0,1,1,0,1,0,1,0,1,0,1,1,0,1,0,1},
        
                // Row 17 (y=408) - 하단 벽들 (세로 연장)
                {1,0,1,0,0,0,0,1,0,1,0,1,0,0,0,0,1,0,1},
        
                // Row 18 (y=432) - 빈 복도
                {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
        
                // Row 19 (y=456) - 최하단 내부 벽들
                {1,0,1,1,1,0,1,1,1,1,1,1,1,0,1,1,1,0,1},
        
                // Row 20 (y=480) - 하단 외벽
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
            };

            // MazeGrid에 복사 - 이제 21x19로 정확히 복사
            for (int row = 0; row < MAZE_ROWS; row++)
            {
                for (int col = 0; col < MAZE_COLS; col++)
                {
                    MazeGrid[row, col] = simpleMaze[row, col];
                }
            }

            // 미로 데이터 검증
            Console.WriteLine("=== 미로 데이터 검증 ===");
            Console.WriteLine($"미로 크기: {MAZE_ROWS} x {MAZE_COLS}");

            // 몇 개 샘플 위치 확인
            Console.WriteLine($"(1,1): {MazeGrid[1, 1]} (통로여야 함)");
            Console.WriteLine($"(0,0): {MazeGrid[0, 0]} (벽이어야 함)");
            Console.WriteLine($"(10,0): {MazeGrid[10, 0]} (터널 - 통로여야 함)");
        }

        public static Form1 Instance;

        public Form1()
        {
            Instance = this; // 생성자에 추가

            InitializeComponent();
            InitializeGame();
            InitializeSimpleMazeGrid();
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
            spawnPos = new PointF(1 * CELL_SIZE, 1 * CELL_SIZE);
            player = new Player(spawnPos, spawnDir);
            player.Died += OnPlayerDied;

            // 타이머 설정
            timer1.Interval = 50;
            timer1.Tick += GameLoop_Tick;
            timer1.Start();

            // 초기 위치 설정 - 확실히 통로인 곳으로
            ghostPos = new Point(9, 12); // 중앙 통로 (행12, 열9)
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

            gameFrameCounter++;
            player.Update(timer1.Interval);
            UpdatePacmanView();

            // 유령을 매 프레임마다 업데이트
            UpdateGhost();

            CheckCollisions();
        }

        private void UpdatePacmanView()
        {
            if (pictureBoxPacman != null)
            {
                pictureBoxPacman.Location = Point.Round(player.Position);

                int dirIdx = (int)player.Facing;
                if (dirIdx >= 0 && dirIdx < 4)
                {
                    pictureBoxPacman.Image = sprites[dirIdx, player.FrameIndex];
                }
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
                    if (stateTimer >= 100) // 더 길게
                    {
                        currentGhostState = GhostState.Chase;
                        stateTimer = 0;
                        Console.WriteLine("Ghost state: Chase");
                    }
                    break;

                case GhostState.Chase:
                    if (stateTimer >= 150) // 더 길게
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                        Console.WriteLine("Ghost state: Scatter");
                    }
                    break;

                case GhostState.Frightened:
                    if (stateTimer >= 100)
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                        Console.WriteLine("Ghost state: Scatter (from Frightened)");
                    }
                    break;

                case GhostState.Eaten:
                    currentGhostState = GhostState.Respawning;
                    stateTimer = 0;
                    ghostPos = new Point(9, 12); // 중앙 통로로 이동
                    ghostPath.Clear();
                    Console.WriteLine("Ghost eaten! Respawning at (9,12)");
                    break;

                case GhostState.Respawning:
                    if (stateTimer >= 60)
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                        Console.WriteLine("Ghost respawned");
                    }
                    break;
            }
        }

        private void MoveGhost()
        {
            // 현재 위치가 유효한지 먼저 체크
            if (!IsValidPosition(ghostPos))
            {
                Console.WriteLine($"ERROR: Ghost in wall at ({ghostPos.X}, {ghostPos.Y})!");
                ghostPos = new Point(9, 12); // 중앙 안전한 위치로 이동
                ghostPath.Clear();
                return;
            }

            // 경로가 없으면 새로 계산
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

            // 유령 이동
            if (ghostPath.Count > 0)
            {
                Point newPos = ghostPath.Dequeue();

                if (IsValidPosition(newPos))
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

        private Point GetGhostTarget()
        {
            switch (currentGhostState)
            {
                case GhostState.Chase:
                    if (IsValidPosition(pacmanPos))
                        return pacmanPos;
                    else
                        return new Point(9, 15);

                case GhostState.Scatter:
                    return new Point(1, 1); // 왼쪽 상단

                case GhostState.Frightened:
                    Random rand = new Random();
                    for (int attempts = 0; attempts < 20; attempts++)
                    {
                        int x = rand.Next(1, MAZE_COLS - 1);
                        int y = rand.Next(1, MAZE_ROWS - 1);
                        if (MazeGrid[y, x] == 0)
                            return new Point(x, y);
                    }
                    return new Point(9, 15);

                case GhostState.Respawning:
                    return new Point(9, 12);

                default:
                    return new Point(9, 15);
            }
        }

        private bool IsValidPosition(Point pos)
        {
            return pos.X >= 0 && pos.X < MAZE_COLS &&
                   pos.Y >= 0 && pos.Y < MAZE_ROWS &&
                   MazeGrid[pos.Y, pos.X] == 0;
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
            if (pictureBox1 != null && pictureBoxPacman != null)
            {
                Rectangle ghostBounds = pictureBox1.Bounds;
                Rectangle pacmanBounds = pictureBoxPacman.Bounds;

                if (ghostBounds.IntersectsWith(pacmanBounds))
                {
                    if (currentGhostState == GhostState.Frightened)
                    {
                        currentGhostState = GhostState.Eaten;
                        stateTimer = 0;
                        ghostPath.Clear();
                    }
                    else if (currentGhostState != GhostState.Eaten &&
                             currentGhostState != GhostState.Respawning)
                    {
                        player.Die();
                    }
                }
            }
        }

        private void OnPlayerDied(object sender, EventArgs e)
        {
            if (isGameover) return;

            timer1.Stop();
            var deathTimer = new System.Windows.Forms.Timer { Interval = 1000 };
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
                    if (MazeGrid[ny, nx] == 0)
                    {
                        neighbors.Add(new Point(nx, ny));
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
            if (isGameover) return;

            // 디버그 키
            if (e.KeyCode == Keys.D)
            {
                Console.WriteLine($"\n=== Debug Info ===");
                Console.WriteLine($"Ghost Position: ({ghostPos.X}, {ghostPos.Y}) - Maze Value: {MazeGrid[ghostPos.Y, ghostPos.X]}");
                Console.WriteLine($"Pacman Position: ({pacmanPos.X}, {pacmanPos.Y}) - Maze Value: {MazeGrid[pacmanPos.Y, pacmanPos.X]}");
                Console.WriteLine($"Ghost State: {currentGhostState}");
                Console.WriteLine($"Path Queue Size: {ghostPath.Count}");

                // 유령 주변 지형 확인
                var neighbors = GetNeighbors(ghostPos);
                Console.WriteLine($"Available neighbors: {neighbors.Count}");
                foreach (var n in neighbors)
                {
                    Console.WriteLine($"  -> ({n.X}, {n.Y})");
                }
                return;
            }

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
    }
}