namespace ChatDemo.DAO
{
    public class UsersDB
    {
        protected readonly string _connectionString;

        public UsersDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public virtual bool AddUser(ChatDemo.Data.User user) 
        {
            throw new NotImplementedException();
        }

        public virtual ChatDemo.Data.User GetUserByMailAndPassword()
        {
            throw new NotImplementedException();
        }

        public virtual List<ChatDemo.Data.User> GetContactForUser(string id)
        {
            throw new NotImplementedException();
        }
    }
}
