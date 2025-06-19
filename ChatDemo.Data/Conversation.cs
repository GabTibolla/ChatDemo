namespace ChatDemo.Data
{
    public class Conversation
    {
        public string Id { get; set; }
        public string ContactNumberId { get; set; }
        public string OwnerNumberId { get; set; }
        public string CreatedAt { get; set; }
        public Message? LastMessage { get; set; }
    }
}
