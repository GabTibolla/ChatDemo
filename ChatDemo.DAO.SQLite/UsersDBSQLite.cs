using System.Text;

namespace ChatDemo.DAO.SQLite
{
    public class UsersDBSQLite : ChatDemo.DAO.UsersDB
    {
        public UsersDBSQLite(string stringConnection) : base(stringConnection)
        {
            CreateTableUser();
            CreateTableContacts();
            CreateTableConversations();
            CreateTableMessages();
        }
        public override bool AddUser(ChatDemo.Data.User user)
        {
            var connection = CriarConnection();
            bool retorno = false;

            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Users (Name, Cpf, Password, WebId, NumberId) ");
                sql.Append("VALUES (@name, @cpf, @password, @webId, @numberId)");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@name", user.Name);
                command.Parameters.AddWithValue("@cpf", user.Cpf);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@webId", user.WebId);
                command.Parameters.AddWithValue("@numberId", user.NumberId);

                retorno = command.ExecuteNonQuery() > 0;
                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                throw;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }
        public override ChatDemo.Data.User? GetUserByNumberId(string numberId)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, Cpf, Password, WebId, NumberId ");
                sql.Append("FROM Users ");
                sql.Append("WHERE NumberId = @NumberId");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@NumberId", numberId);
                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                ChatDemo.Data.User? user = null;
                if (reader.Read())
                {
                    user = new ChatDemo.Data.User();
                    user.Id = reader.GetInt32(0);
                    user.Name = reader.GetString(1);
                    user.Cpf = reader.GetString(2);
                    user.Password = reader.GetString(3);
                    user.WebId = reader.GetString(4);
                    user.NumberId = reader.GetString(5);
                }

                return user;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }
        public override ChatDemo.Data.User? GetUserByWebId(string wid)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, Cpf, Password, WebId, NumberId ");
                sql.Append("FROM Users ");
                sql.Append("WHERE WebId = @WebId");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@WebId", wid);
                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                ChatDemo.Data.User? user = null;
                if (reader.Read())
                {
                    user = new ChatDemo.Data.User();
                    user.Id = reader.GetInt32(0);
                    user.Name = reader.GetString(1);
                    user.Cpf = reader.GetString(2);
                    user.Password = reader.GetString(3);
                    user.WebId = reader.GetString(4);
                    user.NumberId = reader.GetString(5);
                }

                return user;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }
        public override ChatDemo.Data.User? GetUserByCpf(string cpf)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, Cpf, Password, WebId, NumberId ");
                sql.Append("FROM Users ");
                sql.Append("WHERE Cpf = @cpf");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@cpf", cpf);
                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                ChatDemo.Data.User? user = null;
                if (reader.Read())
                {
                    user = new ChatDemo.Data.User();
                    user.Id = reader.GetInt32(0);
                    user.Name = reader.GetString(1);
                    user.Cpf = reader.GetString(2);
                    user.Password = reader.GetString(3);
                    user.WebId = reader.GetString(4);
                    user.NumberId = reader.GetString(5);
                }

                return user;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }
        }
        protected Microsoft.Data.Sqlite.SqliteConnection CriarConnection()
        {
            try
            {
                var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
                connection.Open();

                using (var pragmaTimeout = connection.CreateCommand())
                {
                    pragmaTimeout.CommandText = "PRAGMA busy_timeout = 15000;";
                    pragmaTimeout.ExecuteNonQuery();
                }

                using (var pragmaThreads = connection.CreateCommand())
                {
                    pragmaThreads.CommandText = "PRAGMA threads = 2;";
                    pragmaThreads.ExecuteNonQuery();
                }

                using (var pragmaCache = connection.CreateCommand())
                {
                    pragmaCache.CommandText = "PRAGMA cache_size  = 10000;";
                    pragmaCache.ExecuteNonQuery();
                }

                connection.Close();

                return connection;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region [ Criação das tabelas ]

        private void CreateTableUser()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    Cpf TEXT,
                    Password TEXT NOT NULL,
                    WebId TEXT UNIQUE NOT NULL,
                    NumberId TEXT UNIQUE NOT NULL
                )";
            command.ExecuteNonQuery();
            connection.Close();
        }
        private void CreateTableContacts()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Contacts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Alias TEXT,
                    OwnerNumberId TEXT NOT NULL,
                    ContactNumberId TEXT NOT NULL,
                    UnreadMessages INTEGER,
                    UNIQUE(OwnerNumberId, ContactNumberId)
                )";
            command.ExecuteNonQuery();
            connection.Close();
        }
        private void CreateTableConversations()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Conversations (
                    Id TEXT PRIMARY KEY,
                    ContactNumberId TEXT NOT NULL,
                    OwnerNumberId TEXT NOT NULL,
                    JsonEvent TEXT DEFAULT NULL,
                    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";

            command.ExecuteNonQuery();
            connection.Close();
        }
        private void CreateTableMessages()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ConversationId TEXT NOT NULL,
                    SenderNumberId TEXT NOT NULL,
                    WebId TEXT NOT NULL,
                    Text TEXT,
                    DateTime TEXT,
                    Status INTEGER DEFAULT 0
                )";
            command.ExecuteNonQuery();
            connection.Close();
        }

        #endregion
    }
}
