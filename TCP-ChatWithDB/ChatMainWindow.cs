using System;
using System.Text.Json;
using System.Windows.Forms;
using System.Threading;
using ChatWithDBServer;
using static TCP_ChatWithDB.ChatClient;
using System.Net;

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
            MainWindow = this;
            ChatMessageModel[] chat = null;
            ServerResponse = SendMessageAsync("CH").Result;
            
            try
            {
                chat = JsonSerializer.Deserialize<ChatMessageModel[]>(ServerResponse);
            }
            catch (System.Text.Json.JsonException ex)
            {
                ShowExceptionMessage(ex);
            }
            ChatHistory.Items.Clear();
            for (int i = 0; i < chat.Length; i++)
            {
                ChatHistory.Items.Add(GetFormattedMessage(chat[i]));
            }
            OnlineStatus = false;
        }

        private void MessageBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!OnlineStatus) return;
            if (e.KeyCode == Keys.Enter)
            {
                ChatMessageModel message = CreateMessageObject(MessageBox.Text);
                ChatHistory.Items.Add(GetFormattedMessage(message));
                MessageBox.Text = string.Empty;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!OnlineStatus)
            {
                CreateUser (txtUser.Text);
                txtUser.ReadOnly = true;
                OnlineStatus = true;
                lblStatus.Text = "ONLINE";
                btnConnect.Text = "Выйти из чата";
                Thread thread = new Thread(DoOnlineLoop);
                thread.Start();
            }
            else 
            {
                OnlineStatus = false;
                lblStatus.Text = "OFFLINE";
                btnConnect.Text = "Войти в чат";
                txtUser.ReadOnly = false;
            }
        }
        public ListBox getChatHistory()
        {
            return ChatHistory;
        }

        private void ChatMainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnlineStatus = false;
        }
    }
}