namespace ChatDemo.DAO
{
    public class MessagesDB
    {
        protected readonly string _connectionString;
        public MessagesDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static ChatDemo.DAO.MessagesDB? Create(string className, string projectName, string connectionString)
        {
            var type = Type.GetType($"{className}, {projectName}", true);
            var instance = Activator.CreateInstance(type, connectionString);

            return (ChatDemo.DAO.MessagesDB?)instance;
        }

        public virtual bool AddMessage(ChatDemo.Data.Message message, string ContactNumberId)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateStatusMessageToRead(string conversationId, string SenderNumberId)
        {
            throw new NotImplementedException();
        }

        protected virtual void CreateContactAndUpdateConversation(ChatDemo.Data.Message message, string ContactNumberId)
        {
            throw new NotImplementedException();
        }

        public virtual List<ChatDemo.Data.Message>? GetMessages(string conversationId)
        {
            throw new NotImplementedException();
        }
    }
}
