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
        public static User user = new();
        public static string serverResponse { get; set; }
        public static Boolean OnlineStatus { get; set; }
        public static ChatMainWindow MainWindow { get; set; }


        public static void CreateUser(string name)
        {
            user.IP = GetIPAddress().ToString();
            user.Name = name;
        }
        
        public static ChatMessageModel CreateMessageObject(string msg)
        {
            
            ChatMessageModel message = new ChatMessageModel();
            message.Text = msg;
            message.user = user;
            message.DateTimeStamp = DateTime.Now.ToString();
            string msgJson = JsonSerializer.Serialize (message);
            serverResponse = SendMessageAsync("UM" + msgJson).Result;
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
                if (serverResponse.Length < 2) continue;
                if (serverResponse.Substring(0,2)=="NM") //new messsage is received
                {
                    ChatMessageModel message = JsonSerializer.Deserialize<ChatMessageModel>(serverResponse.Substring(2));
                    if (message.user.Name != user.Name)
                    {
                        System.Windows.Forms.ListBox ChatHistory = MainWindow.getChatHistory();
                        ChatHistory.Invoke((MethodInvoker)delegate
                        {
                            ChatHistory.Items.Insert(0, message.user.Name + " написал в " + message.DateTimeStamp + " сообщение \"" + message.Text + "\"");
                        });
                    }

                }
                serverResponse = "";
                Thread.Sleep(1000);
                //OnlineStatus = false;
            }
        }
        
        public static void SendUserStatus (Boolean online)
        {
            string msgJson = JsonSerializer.Serialize(user);

            serverResponse = SendMessageAsync((online?"ON":"OF") + msgJson).Result;

        }

        public static async Task<string> SendMessageAsync(string msg)
        {
            //string serverResponse = "";


            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync ("192.168.0.103", 8080).ConfigureAwait(false);

            // получаем NetworkStream для взаимодействия с сервером
            var stream = tcpClient.GetStream();

            // буфер для входящих данных
            var response = new List<byte>();
            int bytesRead = 10; // для считывания байтов из потока
            
            // при отправке добавляем маркер завершения сообщения
            byte[] data = Encoding.UTF8.GetBytes(msg + '\n');
            // отправляем данные
            await stream.WriteAsync (data, 0, data.Length).ConfigureAwait(false);

            // считываем данные до конечного символа
            while ((bytesRead = stream.ReadByte()) != '\n')
            {
                // добавляем в буфер
                response.Add((byte) bytesRead);
            }
            serverResponse = Encoding.UTF8.GetString(response.ToArray());
            Console.WriteLine($"Server response: {serverResponse}");
            response.Clear();

            // отправляем маркер завершения подключения
            await stream.WriteAsync(Encoding.UTF8.GetBytes("STOP\n"), 0, "STOP\n".Length).ConfigureAwait(false);

            return serverResponse;
        }
        public static IPAddress GetIPAddress()
        {
            string host = Dns.GetHostName();
            IPAddress address = Dns.GetHostAddresses(host).First<IPAddress>(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (address != null)
            {
                return address;
            } else
            {
                return null;
            }
        }
    }
}