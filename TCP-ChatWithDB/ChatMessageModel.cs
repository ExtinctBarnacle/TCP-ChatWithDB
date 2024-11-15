namespace ChatWithDBServer
{
    public class ChatMessageModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string DateTimeStamp { get; set; }
        public User user { get; set; }

    }
}
