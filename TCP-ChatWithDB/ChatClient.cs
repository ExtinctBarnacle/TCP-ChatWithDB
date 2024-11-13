using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using TCP_ChatWithDB;

namespace TCP_ChatWithDB
{
    public class ChatClient
    {
        public async void sendMessage(string msg)
        {
            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("192.168.0.103", 8080);

            // получаем NetworkStream для взаимодействия с сервером
            var stream = tcpClient.GetStream();

            // буфер для входящих данных
            var response = new List<byte>();
            int bytesRead = 10; // для считывания байтов из потока

            // считыванием строку в массив байт
            // при отправке добавляем маркер завершения сообщения - \n
            byte[] data = Encoding.UTF8.GetBytes(msg + '\n');
            // отправляем данные
            await stream.WriteAsync(data, 0, data.Length);

            // считываем данные до конечного символа
            while ((bytesRead = stream.ReadByte()) != '\n')
            {
                // добавляем в буфер
                response.Add((byte)bytesRead);
            }
            var translation = Encoding.UTF8.GetString(response.ToArray());
            Console.WriteLine($"Server response: {translation}");
            response.Clear();

            // отправляем маркер завершения подключения - END
            await stream.WriteAsync(Encoding.UTF8.GetBytes("END\n"), 0, "END\n".Length);
            Console.WriteLine("Все сообщения отправлены");
        }
    }
}