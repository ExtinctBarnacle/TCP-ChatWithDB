using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCP_ChatWithDB
{
    public partial class ChatMainWindow : Form
    {
        public ChatMainWindow()
        {
            InitializeComponent();
        }

        private void ChatMainWindow_Load(object sender, EventArgs e)
        {

        }

        private void MessageBox_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void ChatHistory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void MessageBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChatClient сhatClient = new ChatClient();
                сhatClient.sendMessage(MessageBox.Text);
                ChatHistory.Items.Add(DateTime.Now.ToString() + " " + MessageBox.Text);
                MessageBox.Text = string.Empty;
            }
        }
    }
}
