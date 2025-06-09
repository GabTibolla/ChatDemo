using ChatDemo.Data;
using System.Text;

namespace ChatDemo.DAO.SQLite
{
    public class UsersDBSQLite : ChatDemo.DAO.UsersDB
    {
        public UsersDBSQLite(string stringConnection) : base(stringConnection)
        {
            CreateTableUser();
        }

        public override bool AddUser(ChatDemo.Data.User user)
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            bool retorno = false;

            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Users (Name, Cpf, Password, WebId) ");
                sql.Append("VALUES (@name, @cpf, @password, @webId)");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@name", user.Name);
                command.Parameters.AddWithValue("@cpf", user.Cpf);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@webId", user.Password);

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

        public override ChatDemo.Data.User? GetUserByCpf(string cpf)
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, Cpf, Password, WebId ");
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
                    Password TEXT,
                    WebId TEXT UNIQUE
                )";
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
