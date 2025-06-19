using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KW_Pacman
{
    internal class MazeManager
    {
        public int[,] LoadMaze(int stage, bool useSeed = false, int seed = 0) {
            if (useSeed) {
                SeedGenerator generator = new SeedGenerator(seed); //시드 기반 랜덤 미로 생성
                return generator.GenerateMaze(20, 15); //20x15 크기의 랜덤 미로
            }

            switch (stage) { //스테이지 고정 미로 반환
                case 1:
                    return new int[,] {
                        {1,1,1,1,1,1,1,1},
                        {1,0,0,0,1,0,0,1},
                        {1,0,1,0,1,0,1,1},
                        {1,0,1,0,0,0,1,1},
                        {1,0,1,1,1,0,1,1},
                        {1,0,0,0,1,0,0,1},
                        {1,1,1,0,1,1,0,1},
                        {1,1,1,0,1,1,0,1}
                    };
                case 2:
                    return new int[,] {
                        {1,1,1,1,1,1,1,1},
                        {1,0,1,0,0,0,0,1},
                        {1,0,1,0,1,1,0,1},
                        {1,0,1,0,1,1,0,1},
                        {1,0,0,0,0,0,0,1},
                        {1,1,1,1,1,1,0,1},
                        {1,0,0,0,0,1,0,1},
                        {1,1,1,1,1,1,0,1}
                    };
                default:
                    throw new ArgumentException("해당 스테이지는 존재하지 않습니다.");
            }
        }
    }
}
