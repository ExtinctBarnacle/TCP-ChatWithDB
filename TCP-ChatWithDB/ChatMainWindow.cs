using System;
using System.Windows.Forms;
using System.Threading;
using ChatWithDBServer;
using static TCP_ChatWithDB.ChatClient;

namespace TCP_ChatWithDB
{
    public partial class ChatMainWindow : Form
    {
        // счётчик сообщений в чате, чтобы цикл формы мог проверить количество новых сообщений
        private int ChatHistoryMessagesCount = 0;

        private int OldChatHistoryMessagesCount = 0;

        static object lockObject = new object();

        public ChatMainWindow()
        {
            InitializeComponent();
        }

        // загрузка формы - запрос истории чата у сервера, добавление истории в окно чата
        private void ChatMainWindow_Load(object sender, EventArgs e)
        {
            ServerResponse = SendMessageAsync("CH").Result;
            IsHistoryLoaded = LoadChatHistory();
            CreateUser(txtUser.Text == string.Empty ? "test" : txtUser.Text);
            OnlineStatus = false;
        }

        // событие нажатия клавиши Enter в поле сообщения - добавляет новое сообщение в окно чата и отправляет его на сервер
        private void MessageBox_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (!OnlineStatus || MessageBox.Text == string.Empty) return;
            if (e.KeyCode == Keys.Enter)
            {
                ChatMessageModel message = CreateMessageObject(MessageBox.Text);
                ChatClient.AddNewMessageToChatHistory(message);
                AddNewMessageToChatHistory(ChatClient.ChatHistory[ChatClient.ChatHistory.Count - 1]);
                MessageBox.Text = string.Empty;
            }
        }
        //событие клика по кнопке BtnConnect - войти в чат / выйти из чата
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            string serverResponse;
            if (!OnlineStatus)
            {
                 serverResponse = SendUserStatus(!OnlineStatus);
                 if (serverResponse.Length > 0)
                  {
                    txtUser.ReadOnly = true;
                    if (!IsHistoryLoaded)
                    {
                        IsHistoryLoaded = LoadChatHistory();
                    }
                    OnlineStatus = !OnlineStatus;
                    Thread chatClientLoop = new Thread(DoOnlineLoop);
                    chatClientLoop.Start();
                    Thread mainWindowLoop = new Thread(DoMainWindowLoop);
                    mainWindowLoop.Start();
                }
            }
            else 
            {
                OnlineStatus = false;
                ExceptionMessageShown = false;
                txtUser.ReadOnly = false;
            }
            SetStatusLabels(OnlineStatus);
        }

        // главный цикл формы для проверки количества сообщений в чате (класс ChatClient не уведомляет форму о событиях и связан только с сервером)
        private void DoMainWindowLoop()
        {
            // количество новых сообщений от других клиентов, не добавленных в окно чата
            while (OnlineStatus)
            {
                lock (lockObject)
                {
                    ChatHistoryMessagesCount = ChatClient.ChatHistory.Count;
                    string name = ChatClient.ChatHistory[ChatClient.ChatHistory.Count - 1];
                    name = name.Substring(0, name.IndexOf('t') - 1);
                    if (ChatHistoryMessagesCount > OldChatHistoryMessagesCount && !string.Equals(name, ChatClient.User.Name))
                    {
                        AddNewMessageToChatHistory(ChatClient.ChatHistory[ChatHistoryMessagesCount - 1]);
                    }
                }
            }
        }

        // при закрытии формы прерывает циклы, выполняемые в других потоках
        private void ChatMainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            OnlineStatus = false;
        }
        // метод для установки надписи онлайн / офлайн в форме
        private void SetStatusLabels(Boolean online)
        {
            if (online)
            {
                lblStatus.Text = "ONLINE";
                btnConnect.Text = "Выйти из чата";
            }
            else 
            {
                lblStatus.Text = "OFFLINE";
                btnConnect.Text = "Войти в чат";
            }
        }

        // метод добавляет новое сообщение пользователя в окно чата
        private void AddNewMessageToChatHistory(string message)
        {
                ChatHistory.Invoke((MethodInvoker) delegate
                {
                    ChatHistory.Items.Add(message);
                    ChatHistory.SelectedIndex = ChatHistory.Items.Count - 1;
                    OldChatHistoryMessagesCount = ChatHistoryMessagesCount;
                    Interlocked.Increment(ref ChatHistoryMessagesCount);
                    //ChatHistoryMessagesCount++;
                });
        }
        // загрузка истории чата, если сервер доступен
        private Boolean LoadChatHistory()
        {
            IsHistoryLoaded = ChatClient.LoadChatHistory();
            if (IsHistoryLoaded)
            {
                ChatHistory.Items.Clear();
                for (int i = 0; i < ChatClient.ChatHistory.Count; i++)
                {
                        ChatHistory.Items.Add(ChatClient.ChatHistory[i]);
                }
                ChatHistoryMessagesCount = OldChatHistoryMessagesCount = ChatClient.ChatHistory.Count;
                ChatHistory.SelectedIndex = ChatHistory.Items.Count - 1;
                return true;
            }
            return false;
        }

        // меняем имя пользователя при вводе нового в текстовом поле
        private void txtUser_TextChanged(object sender, EventArgs e)
        {
            ChatClient.User.Name = txtUser.Text;
        }
    }
}
