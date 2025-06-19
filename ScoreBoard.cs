using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace KW_Pacman
{
    public partial class ScoreBoard : Form
    {
        //스코어 파일의 각 라인은 유저 이름과 점수를 저장합니다
        private string scoreFile = "score.csv";

        private string fullPath;
        private string lvwCol_1 = "UserRank";
        private string lvwCol_2 = "UserName";
        private string lvwCol_3 = "UserScore";

        public ScoreBoard()
        {
            InitializeComponent();

            ofd.InitialDirectory = getInitialPath();
            ofd.FileName = scoreFile;
            fullPath = ofd.InitialDirectory + scoreFile;
        }

        //스코어 파일의 경로를 가져옵니다
        private string getInitialPath()
        {
            string[] temp = AppDomain.CurrentDomain.BaseDirectory.Split('\\');
            string path = "";
            for (int i = 0; i < temp.Length - 4; i++)
                path += (temp[i] + '\\');

            return path;
        }

        //스코어보드 폼을 초기화합니다
        private void ScoreBoard_Load(object sender, EventArgs e)
        {
            lvwScoreBoard.View = View.Details;
            lvwScoreBoard.Columns.Add(lvwCol_1, "등수");
            lvwScoreBoard.Columns.Add(lvwCol_2, "이름");
            lvwScoreBoard.Columns.Add(lvwCol_3, "점수");
            lvwScoreBoard.Columns.Add("last", "last");

            lvwScoreBoard.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            lvwScoreBoard.Columns[1].Width = 200;
            lvwScoreBoard.Columns[2].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            lvwScoreBoard.Columns.RemoveByKey("last");

            lvwScoreBoard.Columns[0].TextAlign = HorizontalAlignment.Left;
            lvwScoreBoard.Columns[1].TextAlign = HorizontalAlignment.Center;
            lvwScoreBoard.Columns[2].TextAlign = HorizontalAlignment.Center;

            setScoreBoard();
        }

        //스코어보드 리스트를 로드합니다
        private void setScoreBoard()
        {
            var scoreList = getScores();
            for (int i = 0; i < scoreList.Count; i++)
            {
                var lvwItem = new ListViewItem(new string[lvwScoreBoard.Columns.Count]);

                for (int k = 0; k < lvwScoreBoard.Columns.Count; k++)
                    lvwItem.SubItems[k].Name = lvwScoreBoard.Columns[k].Name;

                lvwItem.SubItems[lvwCol_1].Text = (i + 1).ToString();
                lvwItem.SubItems[lvwCol_2].Text = scoreList[i][0];
                lvwItem.SubItems[lvwCol_3].Text = scoreList[i][1];

                lvwScoreBoard.Items.Add(lvwItem);
            }

        }

        //스코어 파일을 읽고 반환합니다
        private List<string[]> getScores()
        {
            StreamReader sr = new StreamReader(fullPath);
            var csvList = new List<string[]>();
            try
            {
                Text = Path.GetFileName(fullPath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    csvList.Add(line.Split(','));
                }
            }
            catch (Exception ex)
            {
                Text = "";
                MessageBox.Show(ex.Message, "스코어보드 파일을 찾을 수 없습니다.",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sr.Close();
            }

            //스코어보드 정렬
            csvList.Sort((string[] a, string[] b) => {
                int t = int.Parse(a[1]), s = int.Parse(b[1]);
                if (t.Equals(s)) return a[0].CompareTo(b[0]);
                return t.CompareTo(s) * -1; 
            });

            return csvList;
        }

        //스코어보드 폼 닫기
        private void btnReturn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
