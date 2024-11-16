using System;
using System.Text.Json;
using System.Windows.Forms;
using System.Threading;
using ChatWithDBServer;

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
            ChatMessageModel[] history = JsonSerializer.Deserialize<ChatMessageModel[]> (ChatClient.serverResponse);
            ChatHistory.Items.Clear();
            for (int i = history.Length - 1; i > -1; i--)
            {
                ChatHistory.Items.Add(ChatClient.GetFormattedMessage(history[i]));
            }
            ChatClient.OnlineStatus = false;
        }

        private void MessageBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!ChatClient.OnlineStatus) return;
            if (e.KeyCode == Keys.Enter)
            {
                ChatMessageModel message = ChatClient.CreateMessageObject(MessageBox.Text);
                ChatHistory.Items.Insert(0, ChatClient.GetFormattedMessage(message));
                MessageBox.Text = string.Empty;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!ChatClient.OnlineStatus)
            {
                ChatClient.CreateUser (txtUser.Text);
                txtUser.ReadOnly = true;
                ChatClient.OnlineStatus = true;
                lblStatus.Text = "ONLINE";
                btnConnect.Text = "Disconnect";
                Thread thread = new Thread(ChatClient.DoOnlineLoop);
                thread.Start();
            }
            else 
            {
                ChatClient.OnlineStatus = false;
                lblStatus.Text = "OFFLINE";
                btnConnect.Text = "Connect";
                txtUser.ReadOnly = false;
            }
        }
        public ListBox getChatHistory()
        {
            return ChatHistory;
        }
    }
}