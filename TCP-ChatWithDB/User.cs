namespace ChatWithDBServer
{
    public class User
    {
        // пока не используется - зарезервирован, если понадобится сохранять пользователей в БД
        public int Id { get; set; }

        // имя пользователя, которое он вводит в текстовом поле. Может меняться между подключениями к серверу
        public string Name { get; set; }

        // IP пользователя получает клиент, вызывая метод Dns.GetHostAddresses
        public string IP { get; set; }
    }
}
