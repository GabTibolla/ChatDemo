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

        public virtual List<ChatDemo.Data.Messages>? GetMessagesByNumberId(ChatDemo.Data.Contacts contact)
        {
            throw new NotImplementedException();
        }
    }
}
