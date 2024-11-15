using System;
using System.Text.Json;
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
            ChatClient.serverResponse = ChatClient.SendMessageAsync("CH").Result;
            //while (сhatClient.serverResponse == "") { }
            string[] history = JsonSerializer.Deserialize<string[]> (ChatClient.serverResponse);
            // Array.Reverse (history);
            //ChatHistory.Items.AddRange(history);
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
            if (e.KeyCode == Keys.Enter)
            {
                //ChatClient.user.Name = txtUser.Text;
                ChatClient.CreateMessageObject(MessageBox.Text);
                ChatHistory.Items.Insert(0, DateTime.Now.ToString() + " " + MessageBox.Text);
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
                ChatClient.DoOnlineLoop();
            }
            else 
            {
                ChatClient.OnlineStatus = false;
                btnConnect.Text = "OFFLINE";
            }
        }
    }
}