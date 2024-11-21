using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ChatWithDBServer;
using System.Text.Json;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TCP_ChatWithDB
{
    // главный класс клиента чата - отправляет запросы серверу, принимает ответы сервера, уведомляет об ошибках
    public class ChatClient
    {
        // объект пользователя (имя, IP)
        protected internal static User User { get; set; }
        // ответ сервера: первые 2 символа - код ответа, далее тело запроса в JSON
        protected internal static string ServerResponse { get; set; }
        // статус клинета - онлайн (получает корректные ответы на запросы сервера) или офлайн (сервер недоступен или пользователь вышел из чата)
        protected internal static Boolean OnlineStatus { get; set; }
        //было ли показано сообщение о последнем сбое (чтобы сообщение не выпадало много раз)
        protected internal static Boolean ExceptionMessageShown { get; set; }
        // загружена ли история переписки из БД?
        protected internal static Boolean IsHistoryLoaded { get; set; }
        // пауза в работе главного цикла клиента
        protected internal static int PauseForOnlineLoop { get; set; }
        //история чата в виде списка классов сообщений
        protected internal static List<ChatMessageModel> ChatHistory { get; set; }
        // для блокировки доступа в метод AddNewMessageToChatHistory
        static object lockObject = new object();

        static ChatClient() {
            User = new User();
            ServerResponse = string.Empty;
            OnlineStatus = false;
            ExceptionMessageShown  = false;
            IsHistoryLoaded = false;
            PauseForOnlineLoop = 1000;
            ChatHistory = new List<ChatMessageModel>();
        }

        // создаёт объект пользователя
        protected internal static void CreateUser (string name)
        {
            User.IP = GetEthernetIPAddress();
            User.Name = name;
        }

        // метод создаёт объект сообщения, которое отправляется на сервер
        protected internal static ChatMessageModel CreateMessageObject(string message)
        {
            ChatMessageModel messageModel = new ChatMessageModel();
            messageModel.Text = message;
            messageModel.User = User;
            messageModel.DateTimeStamp = DateTime.Now.ToString();
            string msgJson = JsonSerializer.Serialize(messageModel);
            ServerResponse = SendMessageAsync("UM" + msgJson).Result;
            return messageModel;
        }

        // главный цикл, в котором клиент отправляет статус серверу и принимает ответы сервера
        protected internal static void DoOnlineLoop()
        {
            while (OnlineStatus)
            {
                SendUserStatus (true);
                // некорректное сообщение сервера
                if (ServerResponse.Length < 2) continue;

                if (ServerResponse.Substring(0, 2) == "ON") continue;

                // если получено новое сообщение в чате (NM - new message)
                if (ServerResponse.Substring(0, 2) == "NM")
                {
                    ChatMessageModel message = JsonSerializer.Deserialize<ChatMessageModel>(ServerResponse.Substring(2));
                    if (message.User.Name != User.Name)
                    {
                        AddNewMessageToChatHistory (message);
                    }
                }
                // очистка ответа сервера
                ServerResponse = string.Empty;
                // пауза в работе цикла
                Thread.Sleep (PauseForOnlineLoop);
            }
            SendUserStatus (false);
        }

        // метод добавляет новое сообщение пользователя в окно чата
        protected internal static void AddNewMessageToChatHistory (ChatMessageModel message)
        {
            //string formattedMessage = GetFormattedMessage (message);
            if (!ChatHistory.Contains(message))
            {
                lock (lockObject) 
                { 
                    ChatHistory.Add(message);
                }
            }
        }
        // загрузка истории чата, если сервер доступен
        protected internal static Boolean LoadChatHistory()
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
                ChatHistory.Clear();
                for (int i = 0; i < chat.Length; i++)
                {
                    ChatHistory.Add (chat[i]);
                }
                return true;
            }
            return false;
        }

        // сообщает серверу статус клиента - онлайн или офлайн
        protected internal static string SendUserStatus (Boolean online)
        {
            string msgJson = JsonSerializer.Serialize(User);
            return SendMessageAsync ((online ? "ON" : "OF") + msgJson).Result;
        }

        // асинхронный метод для отправки запросов серверу
        protected internal static async Task<string> SendMessageAsync(string msg)
        {
            string serverIP = GetEthernetIPAddress();
            int serverPort = 8080;
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIP, serverPort).ConfigureAwait(false);

                // получаем NetworkStream для взаимодействия с сервером
                var stream = tcpClient.GetStream();

                // буфер для входящих данных
                var response = new List<byte>();
                int bytesRead = 10; // для считывания байтов из потока

                // при отправке добавляем маркер завершения сообщения
                byte[] data = Encoding.UTF8.GetBytes(msg + '\n');
                // отправляем данные
                await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);

                // считываем данные до конечного символа
                while ((bytesRead = stream.ReadByte()) != '\n')
                {
                    // добавляем в буфер
                    response.Add((byte)bytesRead);
                }
                ServerResponse = Encoding.UTF8.GetString(response.ToArray());
                response.Clear();

                // отправляем маркер завершения подключения
                await stream.WriteAsync(Encoding.UTF8.GetBytes("STOP\n"), 0, "STOP\n".Length).ConfigureAwait(false);

                ExceptionMessageShown = false;
                return ServerResponse;
            }
            catch (Exception ex)
            {
                ShowExceptionMessage(ex);
                return string.Empty;
            }
        }

        // возвращает внутренний IP-адрес компьютера (сервер должен быть запущен на нём)
        protected internal static string GetEthernetIPAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint.Address.ToString();
            }
        }

        // формирует сообщение с именем пользователя и временем для добавления в окно чата
        protected internal static string GetFormattedMessage(ChatMessageModel message)
        {
            DateTime dateTime = DateTime.Parse(message.DateTimeStamp);
            return message.User.Name + "\t\t" + dateTime.Hour + ":" + dateTime.Minute + "\t\t" + message.Text;
        }

        // сообщение об ошибке доступа к серверу
        protected internal static void ShowExceptionMessage(Exception e)
        {
            if (!ExceptionMessageShown)
            {
                OnlineStatus = false;
                MessageBox.Show("Сервер не отвечает. Причина: \n" + e.StackTrace, "Чат");
                // чтобы сообщение об ошибке не выскакивало много раз
                ExceptionMessageShown = true;
            }
        }

    }   
}