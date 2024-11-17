using System.Collections.Generic;
using System;

namespace ChatWithDBServer
{
    //класс сообщения - хранит текст сообщения, дату и время отправки, ссылку на отправителя и булевый список пользователей, которые должны получить сообщение
    public class ChatMessageModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string DateTimeStamp { get; set; }
        public User User { get; set; }
        // массив содержит пары: пользователь - получил / не получил данное сообщение
        public Dictionary<string, Boolean> UsersToReceive { get; set; }

        // проверка, все ли пользователи получили данное сообщение
        public Boolean IsReceivedByAllUsers()
        {
            foreach (var user in UsersToReceive)
            {
                if (user.Value == false) return false;
            }
            return true;
        }
    }
}
