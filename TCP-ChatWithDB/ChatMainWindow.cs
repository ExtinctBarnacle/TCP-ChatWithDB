using System;
using System.Text.Json;
using System.Windows.Forms;
using System.Threading;

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
            ChatClient.MainWindow = this;
            ChatClient.serverResponse = ChatClient.SendMessageAsync("CH").Result;
            string[] history = JsonSerializer.Deserialize<string[]> (ChatClient.serverResponse);
            // Array.Reverse (history);
            //ChatHistory.Items.AddRange(history);
            ChatHistory.Items.Clear();
            for (int i = history.Length - 1; i > -1; i--)
            {
                if (history[i] == null) history[i] = "";
               ChatHistory.Items.Add(history[i]);
            }
            ChatClient.OnlineStatus = false;
        }

        private void MessageBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void ChatHistory_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void MessageBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!ChatClient.OnlineStatus) return;
            if (e.KeyCode == Keys.Enter)
            {
                //ChatClient.user.Name = txtUser.Text;
                ChatClient.CreateMessageObject(MessageBox.Text);
                ChatHistory.Items.Insert(0, txtUser.Text + " " + DateTime.Now.ToString() + " " + MessageBox.Text);
                MessageBox.Text = string.Empty;
            }
        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!ChatClient.OnlineStatus)
            {
                ChatClient.CreateUser (txtUser.Text);
                txtUser.ReadOnly = true;
                ChatClient.OnlineStatus = true;
                btnConnect.Text = "ONLINE";
                Thread thread = new Thread(ChatClient.DoOnlineLoop);
                thread.Start();
            }
            else 
            {
                ChatClient.OnlineStatus = false;
                btnConnect.Text = "OFFLINE";
            }
        }
        public ListBox getChatHistory()
        {
            return ChatHistory;
        }
    }
}