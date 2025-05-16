using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KW_Pacman
{
    internal class SeedGenerator
    {
        private int seed;
        private Random rand;

        public SeedGenerator(int seed) {
            this.seed = seed;
            rand = new Random(seed); //시드를 기반으로 랜덤 생성기 초기화
        }

        public int[,] GenerateMaze(int width, int height)
        {
            int[,] maze = new int[height, width];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        maze[y, x] = 1; //테두리는 벽으로 고정
                    }
                    else
                    {
                        maze[y, x] = (rand.NextDouble() < 0.3) ? 1 : 0;
                    }
                }
            }

            maze[0, 1] = 0; //입구
            maze[height - 1, width - 2] = 0; //출구

            return maze;
        }
    }
}
