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
        public static User User { get; set; }
        // ответ сервера: первые 2 символа - код ответа, далее тело запроса в JSON
        public static string ServerResponse { get; set; }
        // статус клинета - онлайн (получает корректные ответы на запросы сервера) или офлайн (сервер недоступен или пользователь вышел из чата)
        public static Boolean OnlineStatus { get; set; }
        //было ли показано сообщение о последнем сбое (чтобы сообщение не выпадало много раз)
        public static Boolean ExceptionMessageShown { get; set; }
        // загружена ли история переписки из БД?
        public static Boolean IsHistoryLoaded { get; set; }
        // пауза в работе главного цикла клиента
        public static int PauseForOnlineLoop { get; set; }

        public static List<string> ChatHistory { get; set; }

        static ChatClient() {
            User = new User();
            ServerResponse = string.Empty;
            OnlineStatus = false;
            ExceptionMessageShown  = false;
            IsHistoryLoaded = false;
            PauseForOnlineLoop = 1000;
            List<string> ChatHistory = new List<string>();
        }

        // создаёт объект пользователя
        public static void CreateUser(string name)
        {
            User.IP = GetEthernetIPAddress();
            User.Name = name;
        }

        // метод создаёт объект сообщения, которое отправляется на сервер
        public static ChatMessageModel CreateMessageObject(string message)
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
        public static void DoOnlineLoop()
        {
            while (OnlineStatus)
            {
                SendUserStatus(true);
                // некорректное сообщение сервера
                if (ServerResponse.Length < 2) continue;

                // если получено новое сообщение в чате (NM - new message)
                if (ServerResponse.Substring(0, 2) == "NM")
                {
                    ChatMessageModel message = JsonSerializer.Deserialize<ChatMessageModel>(ServerResponse.Substring(2));
                    if (message.User.Name != User.Name)
                    {
                        AddNewMessageToChatHistory(message);
                    }
                }
                // очистка ответа сервера
                ServerResponse = string.Empty;
                //SetStatusLabel(OnlineStatus);
                // пауза в работе цикла
                Thread.Sleep(PauseForOnlineLoop);
            }
            SendUserStatus(false);
            //SetStatusLabel(OnlineStatus);
        }

        /* 
       ** метод добавляет новое сообщение пользователя в окно чата
       */
        public static void AddNewMessageToChatHistory (ChatMessageModel message)
        {
            string formattedMessage = GetFormattedMessage (message);
            if (!(ChatHistory.Contains (formattedMessage)))
            {
                ChatHistory.Add(formattedMessage);
            }
        }
        // загрузка истории чата, если сервер доступен
        public static Boolean LoadChatHistory()
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
                    ChatHistory.Add(GetFormattedMessage(chat[i]));
                }
                return true;
            }
            return false;
        }

        // сообщает серверу статус клиента - онлайн или офлайн
        public static void SendUserStatus(Boolean online)
        {
            string msgJson = JsonSerializer.Serialize(User);
            ServerResponse = SendMessageAsync((online ? "ON" : "OF") + msgJson).Result;
        }

        // асинхронный метод для отправки запросов серверу
        public static async Task<string> SendMessageAsync(string msg)
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
        public static string GetEthernetIPAddress()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                //MessageBox.Show(endPoint.Address.ToString());
                return endPoint.Address.ToString();
            }
        }

        // формирует сообщение с именем пользователя и временем для добавления в окно чата
        public static string GetFormattedMessage(ChatMessageModel message)
        {
            DateTime dateTime = DateTime.Parse(message.DateTimeStamp);
            return message.User.Name + "\t\t" + dateTime.Hour + ":" + dateTime.Minute + "\t\t" + message.Text;
        }

        // сообщение об ошибке доступа к серверу
        public static void ShowExceptionMessage(Exception e)
        {
            if (!ExceptionMessageShown)
            {
                OnlineStatus = false;
                MessageBox.Show("Сервер не отвечает. Причина: \n" + e.StackTrace, "Чат");
                // чтобы сообщение об ошибке не выскакивало много раз
                ExceptionMessageShown = true;
                //SetStatusLabel(OnlineStatus);
            }
        }

    }   
}