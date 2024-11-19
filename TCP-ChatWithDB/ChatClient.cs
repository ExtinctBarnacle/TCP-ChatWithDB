using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ChatWithDBServer;
using System.Text.Json;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TCP_ChatWithDB
{
    public class ChatClient
    {
        public static User User = new();
        public static string ServerResponse { get; set; }
        public static Boolean OnlineStatus { get; set; }

        public static Boolean ExceptionMessageShown = false;
        public static ChatMainWindow MainWindow { get; set; }

        public static int PauseForOnlineLoop = 1000;

        public static void CreateUser (string name)
        {
            User.IP = GetEthernetIPAddress();
            User.Name = name;
        }
        
        public static ChatMessageModel CreateMessageObject (string msg)
        {
            ChatMessageModel message = new ChatMessageModel();
            message.Text = msg;
            message.User = User;
            message.DateTimeStamp = DateTime.Now.ToString();
            string msgJson = JsonSerializer.Serialize (message);
            ServerResponse = SendMessageAsync("UM" + msgJson).Result;
            return message;
        }
        public static void DoOnlineLoop()
        {
            while (true)
            {
                if (OnlineStatus == false) 
                {
                    SendUserStatus (false);
                    break; 
                }
                SendUserStatus (true);
                if (ServerResponse.Length < 2) continue;
                //new messsage is received
                if (ServerResponse.Substring(0,2)=="NM") 
                {
                    ChatMessageModel message = JsonSerializer.Deserialize<ChatMessageModel>(ServerResponse.Substring(2));
                    if (message.User.Name != User.Name)
                    {
                        AddNewMessageToChatHistory (message);
                    }
                }
                ServerResponse = string.Empty;
                Thread.Sleep(PauseForOnlineLoop);
            }
        }
        
        public static void SendUserStatus (Boolean online)
        {
            string msgJson = JsonSerializer.Serialize(User);
            ServerResponse = SendMessageAsync((online ? "ON" : "OF") + msgJson).Result;
        }

        public static async Task<string> SendMessageAsync (string msg)
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
        //public static string GetIPAddress()
        //{
        //    string host = Dns.GetHostName();
        //    IPAddress address = Dns.GetHostAddresses(host).First<IPAddress>(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        //        return address == null ? string.Empty : address.ToString();
            
        //}

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
        public static string GetFormattedMessage(ChatMessageModel message)
        {
            return message.User.Name + " написал в " + message.DateTimeStamp + " сообщение \"" + message.Text + "\"";
        }
        public static void ShowExceptionMessage (Exception e)
        {
            if (!ExceptionMessageShown)
            {
                OnlineStatus = false;
                MessageBox.Show("Сервер не отвечает. Причина: \n" + e.StackTrace, "Чат");
                ExceptionMessageShown = true;
            }
        }
        /* 
        ** метод добавляет новое сообщение пользователя в окно чата
        */
        public static void AddNewMessageToChatHistory(ChatMessageModel message)
        {
            // так как из этого класса нельзя получить доступ к объектам формы, в форме есть метод getChatHistory для получения ссылки на объект с историей чата
            System.Windows.Forms.ListBox ChatHistory = MainWindow.getChatHistory();
            
            // чтобы работать с элементом формы в другом потоке
            ChatHistory.Invoke((MethodInvoker)delegate
            {
                string formattedMessage = GetFormattedMessage(message);
                if (!string.Equals(ChatHistory.Items[0], formattedMessage))
                {
                    ChatHistory.Items.Add(formattedMessage);
                }
            });
        }
    }
}