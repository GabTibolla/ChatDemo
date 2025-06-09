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

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Users (Id, Name, Cpf, Password) ");
                sql.Append("VALUES (@id, @name, @cpf, @password)");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@id", user.Id);
                command.Parameters.AddWithValue("@name", user.Name);
                command.Parameters.AddWithValue("@cpf", user.Cpf);
                command.Parameters.AddWithValue("@password", user.Password);

                retorno = command.ExecuteNonQuery() > 0;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }

            return retorno;
        }

        private void CreateTableUser()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY,
                    Name TEXT,
                    Cpf TEXT,
                    Password TEXT
                )";
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
