namespace ChatDemo.DAO
{
    public class ConversationsDB
    {
        protected readonly string _connectionString;

        public ConversationsDB(string connectionString)
        {
            _connectionString = connectionString;
        }
        public static ChatDemo.DAO.ConversationsDB? Create(string className, string projectName, string connectionString)
        {
            var type = Type.GetType($"{className}, {projectName}", true);
            var instance = Activator.CreateInstance(type, connectionString);

            return (ChatDemo.DAO.ConversationsDB?)instance;
        }
        public virtual bool CreateConversation(ChatDemo.Data.Conversation conversation)
        {
            throw new NotImplementedException();
        }
        public virtual List<ChatDemo.Data.Conversation>? GetConversations(string OwnerNumberId)
        {
            throw new NotImplementedException();
        }
        public virtual ChatDemo.Data.Conversation? GetConversation(string conversationId)
        {
            throw new NotImplementedException();
        }
        public virtual bool UpdateConversation(ChatDemo.Data.Conversation conversation)
        {
            throw new NotImplementedException();
        }
    }
}
