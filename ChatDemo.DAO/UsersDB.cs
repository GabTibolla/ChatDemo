namespace ChatDemo.DAO
{
    public class UsersDB
    {
        protected readonly string _connectionString;

        public UsersDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static ChatDemo.DAO.UsersDB? Create(string className, string projectName, string connectionString)
        {
            var type = Type.GetType($"{className}, {projectName}", true);
            var instance = Activator.CreateInstance(type, connectionString);

            return (ChatDemo.DAO.UsersDB?)instance;
        }

        public virtual bool AddUser(ChatDemo.Data.User user) 
        {
            throw new NotImplementedException();
        }

        public virtual ChatDemo.Data.User? GetUserByCpf(string cpf)
        {
            throw new NotImplementedException();
        }

        public virtual List<ChatDemo.Data.User>? GetContactForUser(string id)
        {
            throw new NotImplementedException();
        }
    }
}
