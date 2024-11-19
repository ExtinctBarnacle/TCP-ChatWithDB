using System;
using System.Windows.Forms;
using System.Threading;
using ChatWithDBServer;
using static TCP_ChatWithDB.ChatClient;
using System.Text.Json;

namespace TCP_ChatWithDB
{
    public partial class ChatMainWindow : Form
    {
        public ChatMainWindow()
        {
            InitializeComponent();
        }

        // загрузка формы - запрос истории чата у сервера, добавление истории в окно чата
        private void ChatMainWindow_Load(object sender, EventArgs e)
        {
            //MainWindow = this;
            ServerResponse = SendMessageAsync("CH").Result;
            IsHistoryLoaded = LoadChatHistory();
            OnlineStatus = false;
        }

        // событие нажатия клавиши Enter в поле сообщения - добавляет новое сообщение в окно чата и отправляет его на сервер
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
        //событие клика по кнопке BtnConnect - войти в чат / выйти из чата
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (!OnlineStatus)
            {
                //SendUserStatus(true);
                //if (ServerResponse.Length > 0)
                {
                    CreateUser(txtUser.Text);
                    txtUser.ReadOnly = true;
                    if (!IsHistoryLoaded)
                    {
                        IsHistoryLoaded = LoadChatHistory();
                    }
                    OnlineStatus = true;
                    lblStatus.Text = "Подключение...";
                    btnConnect.Text = "Выйти из чата";
                    Thread thread = new Thread(DoOnlineLoop);
                    thread.Start();
                } 
            }
            else 
            {
                OnlineStatus = false;
                lblStatus.Text = "OFFLINE";
                btnConnect.Text = "Войти в чат";
                txtUser.ReadOnly = false;
            }
        }

        // при закрытии формы прерывает главный цикл программы
        private void ChatMainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnlineStatus = false;
        }
        // метод для установки надписи онлайн / офлайн в форме
        public void SetStatusLabels(Boolean online)
        {
            if (online) lblStatus.Text = "ONLINE";
            else lblStatus.Text = "OFFLINE";
        }

        /* 
        ** метод добавляет новое сообщение пользователя в окно чата
        */
        public void AddNewMessageToChatHistory(ChatMessageModel message)
        {
            string formattedMessage = GetFormattedMessage(message);
            if (!string.Equals(ChatHistory.Items[0], formattedMessage))
            {
                ChatHistory.Items.Add(formattedMessage);
            }
        }
        // загрузка истории чата, если сервер доступен
        public Boolean LoadChatHistory()
        {
            ChatMessageModel[] chat = null;
            try
            {
                chat = JsonSerializer.Deserialize<ChatMessageModel[]>(ServerResponse);
            }
            catch (System.Text.Json.JsonException ex)
            {
                ShowExceptionMessage(ex);
                return false;
            }
            if (chat != null)
            {
                ChatHistory.Items.Clear();
                for (int i = 0; i < chat.Length; i++)
                {
                    ChatHistory.Items.Add(GetFormattedMessage(chat[i]));
                    ChatHistory.SelectedIndex = ChatHistory.Items.Count - 1;
                }
                return true;
            }
            return false;
        }
    }
}
