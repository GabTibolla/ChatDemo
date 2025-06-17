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

        public virtual bool AddMessage(ChatDemo.Data.Message message)
        {
            throw new NotImplementedException();
        }

        public virtual bool UpdateStatusMessageToRead(ChatDemo.Data.User userFrom, ChatDemo.Data.User userTo)
        {
            throw new NotImplementedException();
        }

        protected virtual void CreateContact(string fromNumberId, string toNumberId, ChatDemo.Data.Message message)
        {
            throw new NotImplementedException();
        }

        public virtual List<ChatDemo.Data.Message>? GetMessagesByNumberId(string fromNumberId, string toNumberId)
        {
            throw new NotImplementedException();
        }
    }
}
