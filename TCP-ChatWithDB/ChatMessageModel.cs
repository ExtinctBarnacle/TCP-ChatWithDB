using System.Collections.Generic;
using System;

namespace ChatWithDBServer
{
    public class ChatMessageModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string DateTimeStamp { get; set; }
        public User user { get; set; }
        public Dictionary<string, Boolean> usersToReceive { get; set; }
        public Boolean isReceivedByAllUsers()
        {
            foreach (var user in usersToReceive)
            {
                if (user.Value == false) return false;
            }
            return true;
        }

        public void setUserReceivedMessage(User user)
        {
            usersToReceive[user.Name] = true;
        }
    }
}
