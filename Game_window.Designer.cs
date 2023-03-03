
namespace Blackjack
{
    partial class Game_window
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
            this.btnStand = new System.Windows.Forms.Button();
            this.btnHit = new System.Windows.Forms.Button();
            this.pnlPlayer = new System.Windows.Forms.Panel();
            this.pnlAi = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // btnStand
            // 
            this.btnStand.Location = new System.Drawing.Point(499, 331);
            this.btnStand.Name = "btnStand";
            this.btnStand.Size = new System.Drawing.Size(140, 77);
            this.btnStand.TabIndex = 0;
            this.btnStand.Text = "Stand";
            this.btnStand.UseVisualStyleBackColor = true;
            // 
            // btnHit
            // 
            this.btnHit.Location = new System.Drawing.Point(163, 331);
            this.btnHit.Name = "btnHit";
            this.btnHit.Size = new System.Drawing.Size(133, 77);
            this.btnHit.TabIndex = 1;
            this.btnHit.Text = "Hit";
            this.btnHit.UseVisualStyleBackColor = true;
            // 
            // pnlPlayer
            // 
            this.pnlPlayer.Location = new System.Drawing.Point(50, 175);
            this.pnlPlayer.Name = "pnlPlayer";
            this.pnlPlayer.Size = new System.Drawing.Size(699, 150);
            this.pnlPlayer.TabIndex = 3;
            // 
            // pnlAi
            // 
            this.pnlAi.Location = new System.Drawing.Point(50, 31);
            this.pnlAi.Name = "pnlAi";
            this.pnlAi.Size = new System.Drawing.Size(699, 150);
            this.pnlAi.TabIndex = 4;
            // 
            // Game_window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pnlAi);
            this.Controls.Add(this.pnlPlayer);
            this.Controls.Add(this.btnHit);
            this.Controls.Add(this.btnStand);
            this.Name = "Game_window";
            this.Text = "Form3";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStand;
        private System.Windows.Forms.Button btnHit;
        private System.Windows.Forms.Panel pnlPlayer;
        private System.Windows.Forms.Panel pnlAi;
    }
}