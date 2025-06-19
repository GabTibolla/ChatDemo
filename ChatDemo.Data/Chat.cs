namespace ChatDemo.Data
{
    public class Chat
    {
        public List<Message>? Messages { get; set; }
        public List<Conversation>? Conversations { get; set; }
        public List<Contacts>? Contacts { get; set; }
        public Conversation? CurrentConversation { get; set; }
        public User? User { get; set; }
    }
}
