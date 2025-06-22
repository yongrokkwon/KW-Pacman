using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KW_Pacman
{
    public partial class ScoreBoard : Form
    {
        private ListView listViewScores;
        private Button btnClose;
        private Button btnClear;
        private Label lblTitle;

        public ScoreBoard()
        {
            InitializeComponent();
            InitializeScoreBoard();
            LoadScores();
        }

        private void InitializeScoreBoard()
        {
            // 폼 설정
            this.Text = "최고 점수";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.Black;

            // 제목 라벨
            lblTitle = new Label()
            {
                Text = "🏆 최고 점수 순위 🏆",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.Yellow,
                BackColor = Color.Black,
                Location = new Point(20, 20),
                Size = new Size(440, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // 점수 목록 표시용 ListView
            listViewScores = new ListView()
            {
                Location = new Point(20, 60),
                Size = new Size(440, 250),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.White,
                ForeColor = Color.Gray,
                Font = new Font("Arial", 10)
            };

            // 컬럼 추가
            listViewScores.Columns.Add("순위", 60, HorizontalAlignment.Center);
            listViewScores.Columns.Add("플레이어", 150, HorizontalAlignment.Left);
            listViewScores.Columns.Add("점수", 100, HorizontalAlignment.Right);
            listViewScores.Columns.Add("날짜", 120, HorizontalAlignment.Center);

            this.Controls.Add(listViewScores);

            // 닫기 버튼
            btnClose = new Button()
            {
                Text = "닫기",
                Location = new Point(300, 320),
                Size = new Size(80, 30),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnClose.Click += BtnClose_Click;
            this.Controls.Add(btnClose);

            // 기록 초기화 버튼
            btnClear = new Button()
            {
                Text = "기록 초기화",
                Location = new Point(390, 320),
                Size = new Size(80, 30),
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnClear.Click += BtnClear_Click;
            this.Controls.Add(btnClear);
        }

        private void LoadScores()
        {
            try
            {
                // 기존 항목 클리어
                listViewScores.Items.Clear();

                // 점수 불러오기
                List<ScoreRecord> scores = ScoreManager.LoadScores();

                if (scores.Count == 0)
                {
                    // 기록이 없을 때
                    ListViewItem noScoreItem = new ListViewItem("-");
                    noScoreItem.SubItems.Add("기록이 없습니다");
                    noScoreItem.SubItems.Add("-");
                    noScoreItem.SubItems.Add("-");
                    noScoreItem.ForeColor = Color.Gray;
                    listViewScores.Items.Add(noScoreItem);
                }
                else
                {
                    // 점수 목록 표시 (상위 10개)
                    for (int i = 0; i < Math.Min(scores.Count, 10); i++)
                    {
                        ScoreRecord score = scores[i];
                        ListViewItem item = new ListViewItem((i + 1).ToString());
                        item.SubItems.Add(score.PlayerName);
                        item.SubItems.Add(score.Score.ToString("N0"));
                        item.SubItems.Add(score.Date.ToString("MM/dd HH:mm"));

                        // 1등은 골드 색상
                        if (i == 0)
                        {
                            item.ForeColor = Color.Gold;
                            item.Font = new Font("Arial", 10, FontStyle.Bold);
                        }
                        // 2등은 실버 색상
                        else if (i == 1)
                        {
                            item.ForeColor = Color.Silver;
                        }
                        // 3등은 브론즈 색상
                        else if (i == 2)
                        {
                            item.ForeColor = Color.Orange;
                        }
                        else
                        {
                            item.ForeColor = Color.White;
                        }

                        listViewScores.Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"점수를 불러오는 중 오류가 발생했습니다: {ex.Message}",
                              "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "정말로 모든 점수 기록을 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
                "기록 삭제 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                ScoreManager.ClearScores();
                LoadScores();
                MessageBox.Show("모든 점수 기록이 삭제되었습니다.", "완료",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 폼이 표시될 때마다 점수 새로고침
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(value);
            if (value && this.Created)
            {
                LoadScores();
            }
        }
    }
}