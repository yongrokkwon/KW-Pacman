using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.IO;

namespace KW_Pacman
{
    internal class ScoreManager
    {
        public int Score {
            get;
            private set;
        }

        public ScoreManager() {
            Score = 0;
        }

        public void ResetScore() {
            Score = 0;
        }

        public void EatDot() {
            Score += 10;
            //PlaySound("");
        }

        public void EatSomething() {
            Score += 50;
            //PlaySound("");
        }

        public void EatSomethingMore() {
            Score += 100;
            //PlaySound("");
        }

        private void PlaySound(string fileName) {
            try
            {
                string path = Path.Combine("Resources", fileName);

                if (File.Exists(path))
                {
                    using (SoundPlayer player = new SoundPlayer(path))
                    {
                        player.Play();
                    }
                }
                else
                {
                    Console.WriteLine($"[사운드 파일 없음] {path}");
                }
            }
            catch (Exception e) {
                Console.WriteLine($"[사운드 오류] {fileName} : {e.Message}");
            }
        }
    }
}
