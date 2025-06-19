//namespace KW_Pacman
//{
//    public static class MazeConsoleTest
//    {
//        public static void Main()
//        {
//            MazeManager mazeManager = new MazeManager();

//            bool useSeed = true;
//            int seed = 42;
//            int stage = 1;

//            int[,] maze = mazeManager.LoadMaze(stage, useSeed, seed);

//            for (int y = 0; y < maze.GetLength(0); y++)
//            {
//                for (int x = 0; x < maze.GetLength(1); x++)
//                {
//                    Console.Write(maze[y, x] == 1 ? "■ " : "□ ");
//                }
//                Console.WriteLine();
//            }

//            Console.WriteLine("\n[엔터를 누르면 종료]");
//            Console.ReadLine();
//        }
//    }
//}

