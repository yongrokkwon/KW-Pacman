namespace KW_Pacman
{
    partial class ScoreBoard
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
            lvwScoreBoard = new ListView();
            ofd = new OpenFileDialog();
            btnReturn = new Button();
            SuspendLayout();
            // 
            // lvwScoreBoard
            // 
            lvwScoreBoard.Dock = DockStyle.Top;
            lvwScoreBoard.Font = new Font("맑은 고딕", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 129);
            lvwScoreBoard.Location = new Point(0, 0);
            lvwScoreBoard.Name = "lvwScoreBoard";
            lvwScoreBoard.Size = new Size(800, 387);
            lvwScoreBoard.TabIndex = 0;
            lvwScoreBoard.UseCompatibleStateImageBehavior = false;
            lvwScoreBoard.View = View.List;
            // 
            // ofd
            // 
            ofd.FileName = "openFileDialog1";
            // 
            // btnReturn
            // 
            btnReturn.Location = new Point(339, 405);
            btnReturn.Name = "btnReturn";
            btnReturn.Size = new Size(98, 33);
            btnReturn.TabIndex = 1;
            btnReturn.Text = "돌아가기";
            btnReturn.UseVisualStyleBackColor = true;
            btnReturn.Click += btnReturn_Click;
            // 
            // ScoreBoard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnReturn);
            Controls.Add(lvwScoreBoard);
            Name = "ScoreBoard";
            Text = "ScoreBoard";
            Load += ScoreBoard_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListView lvwScoreBoard;
        private OpenFileDialog ofd;
        private Button btnReturn;
    }
}