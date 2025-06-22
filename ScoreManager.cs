namespace KW_Pacman
{
    public class ScoreRecord
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }

        public ScoreRecord()
        {
        }

        public ScoreRecord(string playerName, int score, DateTime date)
        {
            PlayerName = playerName;
            Score = score;
            Date = date;
        }

        public override string ToString()
        {
            return $"{PlayerName}: {Score} ({Date:yyyy-MM-dd HH:mm})";
        }
    }

    public static class ScoreManager
    {
        private static readonly string ScoreFileName = "pacman_scores.txt";
        private static readonly string ScoreFilePath = Path.Combine(Application.StartupPath, ScoreFileName);

        // 점수 저장
        public static void SaveScore(string playerName, int score)
        {
            try
            {
                ScoreRecord newScore = new ScoreRecord(playerName, score, DateTime.Now);

                // 기존 점수들 불러오기
                List<ScoreRecord> scores = LoadScores();

                // 새 점수 추가
                scores.Add(newScore);

                // 점수 순으로 정렬 (높은 점수부터)
                scores = scores.OrderByDescending(s => s.Score).ToList();

                // 상위 10개만 유지
                if (scores.Count > 10)
                {
                    scores = scores.Take(10).ToList();
                }

                // 파일에 저장
                SaveScoresToFile(scores);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"점수 저장 중 오류: {ex.Message}");
            }
        }

        // 점수 불러오기
        public static List<ScoreRecord> LoadScores()
        {
            List<ScoreRecord> scores = new List<ScoreRecord>();

            try
            {
                if (File.Exists(ScoreFilePath))
                {
                    string[] lines = File.ReadAllLines(ScoreFilePath);

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            ScoreRecord score = ParseScoreLine(line);
                            if (score != null)
                            {
                                scores.Add(score);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"점수 불러오기 중 오류: {ex.Message}");
            }

            return scores.OrderByDescending(s => s.Score).ToList();
        }

        // 파일에 점수들 저장
        private static void SaveScoresToFile(List<ScoreRecord> scores)
        {
            try
            {
                List<string> lines = new List<string>();

                foreach (ScoreRecord score in scores)
                {
                    lines.Add($"{score.PlayerName}|{score.Score}|{score.Date:yyyy-MM-dd HH:mm:ss}");
                }

                File.WriteAllLines(ScoreFilePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"파일 저장 중 오류: {ex.Message}");
            }
        }

        // 문자열에서 점수 레코드 파싱
        private static ScoreRecord ParseScoreLine(string line)
        {
            try
            {
                string[] parts = line.Split('|');
                if (parts.Length == 3)
                {
                    string playerName = parts[0];
                    int score = int.Parse(parts[1]);
                    DateTime date = DateTime.ParseExact(parts[2], "yyyy-MM-dd HH:mm:ss", null);

                    return new ScoreRecord(playerName, score, date);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"점수 라인 파싱 중 오류: {ex.Message}");
            }

            return null;
        }

        // 점수 파일 초기화
        public static void ClearScores()
        {
            try
            {
                if (File.Exists(ScoreFilePath))
                {
                    File.Delete(ScoreFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"점수 파일 삭제 중 오류: {ex.Message}");
            }
        }

        // 최고 점수 가져오기
        public static ScoreRecord GetHighScore()
        {
            List<ScoreRecord> scores = LoadScores();
            return scores.FirstOrDefault();
        }
    }
}