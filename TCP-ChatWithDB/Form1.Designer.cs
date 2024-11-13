namespace TCP_ChatWithDB
{
    partial class ChatMainWindow
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
            this.MessageBox = new System.Windows.Forms.TextBox();
            this.ChatHistory = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // MessageBox
            // 
            this.MessageBox.AcceptsReturn = true;
            this.MessageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MessageBox.Location = new System.Drawing.Point(41, 12);
            this.MessageBox.Name = "MessageBox";
            this.MessageBox.Size = new System.Drawing.Size(578, 26);
            this.MessageBox.TabIndex = 0;
            this.MessageBox.TextChanged += new System.EventHandler(this.MessageBox_TextChanged);
            this.MessageBox.KeyUp += this.MessageBox_KeyUp;
            // 
            // ChatHistory
            // 
            this.ChatHistory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ChatHistory.FormattingEnabled = true;
            this.ChatHistory.ItemHeight = 20;
            this.ChatHistory.Location = new System.Drawing.Point(41, 44);
            this.ChatHistory.Name = "ChatHistory";
            this.ChatHistory.Size = new System.Drawing.Size(578, 344);
            this.ChatHistory.TabIndex = 1;
            this.ChatHistory.SelectedIndexChanged += new System.EventHandler(this.ChatHistory_SelectedIndexChanged);
            // 
            // ChatMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ChatHistory);
            this.Controls.Add(this.MessageBox);
            this.Name = "ChatMainWindow";
            this.Text = "Chat";
            this.Load += new System.EventHandler(this.ChatMainWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MessageBox;
        private System.Windows.Forms.ListBox ChatHistory;
    }
}

