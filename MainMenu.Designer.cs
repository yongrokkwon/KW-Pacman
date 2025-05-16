namespace KW_Pacman
{
    partial class MainMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Title = new Label();
            btnStart = new Button();
            btnScoreBoard = new Button();
            btnQuit = new Button();
            SuspendLayout();
            // 
            // Title
            // 
            Title.AutoSize = true;
            Title.Font = new Font("맑은 고딕", 36F, FontStyle.Bold, GraphicsUnit.Point, 129);
            Title.Location = new Point(253, 56);
            Title.Name = "Title";
            Title.Size = new Size(305, 65);
            Title.TabIndex = 0;
            Title.Text = "KW-Pacman";
            Title.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnStart
            // 
            btnStart.Font = new Font("맑은 고딕", 18F);
            btnStart.Location = new Point(253, 185);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(307, 60);
            btnStart.TabIndex = 1;
            btnStart.Text = "게임 시작";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnScoreBoard
            // 
            btnScoreBoard.Font = new Font("맑은 고딕", 18F);
            btnScoreBoard.Location = new Point(253, 251);
            btnScoreBoard.Name = "btnScoreBoard";
            btnScoreBoard.Size = new Size(307, 60);
            btnScoreBoard.TabIndex = 1;
            btnScoreBoard.Text = "스코어보드";
            btnScoreBoard.UseVisualStyleBackColor = true;
            btnScoreBoard.Click += btnScoreBoard_Click;
            // 
            // btnQuit
            // 
            btnQuit.Font = new Font("맑은 고딕", 18F);
            btnQuit.Location = new Point(253, 317);
            btnQuit.Name = "btnQuit";
            btnQuit.Size = new Size(307, 60);
            btnQuit.TabIndex = 1;
            btnQuit.Text = "게임 종료";
            btnQuit.UseVisualStyleBackColor = true;
            btnQuit.Click += btnQuit_Click;
            // 
            // MainMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnQuit);
            Controls.Add(btnScoreBoard);
            Controls.Add(btnStart);
            Controls.Add(Title);
            Name = "MainMenu";
            Text = "MainMenu";
            Load += MainMenu_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label Title;
        private Button btnStart;
        private Button btnScoreBoard;
        private Button btnQuit;
    }
}