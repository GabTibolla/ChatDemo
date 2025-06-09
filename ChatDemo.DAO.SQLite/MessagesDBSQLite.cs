using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDemo.DAO.SQLite
{
    public class MessagesDBSQLite : ChatDemo.DAO.MessagesDB
    {
        public MessagesDBSQLite(string connectionString) : base(connectionString)
        {
            CreateTableMessages();
        }

        public override List<ChatDemo.Data.Messages>? GetMessagesByNumberId(ChatDemo.Data.Contacts contact)
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            try
            {
                connection.Open();

                string idChat = contact.MyNumberId + contact.NumberId;

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Message, Datetime, FromNumberId, ToNumberId, Sent, IdChat ");
                sql.Append("FROM Messages ");
                sql.Append("WHERE IdChat = @idChat ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@idChat", idChat);
                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                List<ChatDemo.Data.Messages>? messages = new List<Data.Messages>();
                while (reader.Read())
                {
                    var message = new ChatDemo.Data.Messages();
                    message.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    message.Message = reader.IsDBNull(1) ? null : reader.GetString(1);
                    message.Datetime = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2);
                    message.FromNumberId = reader.IsDBNull(3) ? null : reader.GetString(3);
                    message.ToNumberId = reader.IsDBNull(4) ? null : reader.GetString(4);
                    message.Sent = reader.IsDBNull(5) ? false : reader.GetInt32(5) > 0;
                    message.IdChat = reader.IsDBNull(6) ? null : reader.GetString(6);

                    messages.Add(message);
                }

                return messages;
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

        private void CreateTableMessages()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Messages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Message TEXT,
                    DateTime TEXT,
                    FromNumberId TEXT,
                    ToNumberId INTEGER,
                    Sent INTEGER,
                    IdChat TEXT
                )";
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}
