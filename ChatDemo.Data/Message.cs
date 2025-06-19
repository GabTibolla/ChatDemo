namespace ChatDemo.Data
{
    public class Message
    {
        public enum StatusMessage
        {
            None = 0,
            Sent = 1,
            Read = 2
        };


        public int Id { get; set; }
        public string WebId { get; set; }
        public string? ConversationId { get; set; }
        public string? SenderNumberId { get; set; }
        public string? Text { get; set; }
        public DateTime Datetime { get; set; }
        public StatusMessage Status { get; set; }
    }
}
