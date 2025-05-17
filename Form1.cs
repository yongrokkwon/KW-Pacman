using System;
using System.Collections.Generic;
using System.Drawing;

namespace PACKMAN
{
    public partial class Form1 : Form
    {
        // 가상의 미로 크기(추후 실제 맵과 동일하게 맞출 것)
        private const int MAZE_ROWS = 21;
        private const int MAZE_COLS = 19;

        // 0 = 빈칸, 1 = 벽 (예시 미로, 실제 미로로 대체)
        // 미로가 완성되면 여기 데이터만 바꾸면 됨
        private int[,] MazeGrid = new int[MAZE_ROWS, MAZE_COLS];


        // 유령, 팩맨의 위치를 "미로 좌표"로 관리 (그리드 단위)
        private Point ghostPos = new Point(1, 1);   // 예: (1,1)에서 시작
        private Point pacmanPos = new Point(10, 10);

        // 이동경로 (A*로 계산)
        private Queue<Point> ghostPath = new Queue<Point>();

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

            pictureBox1.Image = KW_Pacman.Properties.Resource.RedGhost;
            pictureBoxPacman.BackColor = System.Drawing.Color.Yellow; // 임시 팩맨 색
            pictureBoxPacman.SizeMode = PictureBoxSizeMode.StretchImage;

            // 팩맨/유령 위치 초기화, 미로 위에서 좌표만 초기화
            ghostPos = new Point(1, 1);
            pacmanPos = new Point(10, 10);
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
                    pictureBox1.Image = KW_Pacman.Properties.Resource.RedGhost;
                    //pictureBox1.Left -= 5;
                    if (stateTimer >= 30)
                    {
                        currentGhostState = GhostState.Chase;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Chase:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.RedGhost;
                    //pictureBox1.Left += 5;
                    if (stateTimer >= 30)
                    {
                        currentGhostState = GhostState.Frightened;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Frightened:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.ScaredGhost;
                    //pictureBox1.Left -= 2;
                    if (stateTimer >= 20)
                    {
                        currentGhostState = GhostState.Scatter;
                        stateTimer = 0;
                    }
                    break;

                case GhostState.Eaten:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.GhostEye;
                    pictureBox1.Visible = false;
                    currentGhostState = GhostState.Respawning;
                    stateTimer = 0;
                    break;

                case GhostState.Respawning:
                    pictureBox1.Image = KW_Pacman.Properties.Resource.GhostEye;
                    if (!pictureBox1.Visible)
                        pictureBox1.Visible = true;

                    //if (pictureBox1.Left > 100)
                    //    pictureBox1.Left -= 5;
                    //else if (pictureBox1.Left < 100)
                    //    pictureBox1.Left += 5;
                    //else
                    //{
                    //    currentGhostState = GhostState.Scatter;
                    //    stateTimer = 0;
                    //}
                    currentGhostState = GhostState.Scatter;
                    stateTimer = 0;
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

            // 팩맨 위치(미로 그리드 상)를 주기적으로 업데이트
            pacmanPos = PixelToMaze(pictureBoxPacman.Location);

            // 1. 유령이 팩맨을 향해 길을 찾음 (필요할 때만, ex: 목표 변경시)
            if (ghostPath.Count == 0)
            {
                ghostPath = FindPathAStar(ghostPos, pacmanPos);
            }

            // 2. 유령이 한 칸 이동 (경로가 있으면)
            if (ghostPath.Count > 0)
            {
                ghostPos = ghostPath.Dequeue();
            }

            // 3. 유령 실제 픽셀 위치로 변환해서 pictureBox1 위치 업데이트
            pictureBox1.Location = MazeToPixel(ghostPos);
        }

        // --- A* 알고리즘 구현 ---
        // (나중에 MazeGrid/벽/좌표만 실제 맵으로 연결하면 바로 사용 가능)
        private Queue<Point> FindPathAStar(Point start, Point goal)
        {
            var path = new Queue<Point>();

            // [A* 기본 구조]
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
                    // 시작 위치는 빼고 반환(현재 위치->다음 이동)
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

        // --- 휴리스틱(맨해튼 거리) ---
        private int Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        // --- 인접 칸 반환 (벽은 제외) ---
        private List<Point> GetNeighbors(Point p)
        {
            var dirs = new Point[] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0) }; // 상하좌우
            var neighbors = new List<Point>();
            foreach (var d in dirs)
            {
                int nx = p.X + d.X, ny = p.Y + d.Y;
                if (nx >= 0 && nx < MAZE_COLS && ny >= 0 && ny < MAZE_ROWS && MazeGrid[ny, nx] == 0)
                    neighbors.Add(new Point(nx, ny));
            }
            return neighbors;
        }

        // --- 미로 좌표를 픽셀 위치로 변환하는 함수(미로 완성시 구현) ---
        private Point MazeToPixel(Point mazePos)
        {
            int cellSize = 24; // 예시 (타일 한 칸 크기)
            return new Point(mazePos.X * cellSize, mazePos.Y * cellSize);
        }

        // --- 픽셀 위치를 미로 좌표로 변환하는 함수(팩맨 이동시 사용) ---
        private Point PixelToMaze(Point pixelPos)
        {
            int cellSize = 24;
            return new Point(pixelPos.X / cellSize, pixelPos.Y / cellSize);
        }
    }
}
