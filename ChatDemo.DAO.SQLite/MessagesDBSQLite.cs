using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDemo.DAO.SQLite
{
    public class MessagesDBSQLite : ChatDemo.DAO.MessagesDB
    {
        public MessagesDBSQLite(string connectionString) : base(connectionString)
        {
        }

        public override bool AddMessage(ChatDemo.Data.Messages message)
        {
            var connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Messages (Message, DateTime, FromNumberId, ToNumberId) ");
                sql.Append("VALUES (@Message, @Datetime, @FromNumberId, @ToNumberId) ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();
                command.Parameters.AddWithValue("@Message", message.Message);
                command.Parameters.AddWithValue("@Datetime", message.Datetime);
                command.Parameters.AddWithValue("@FromNumberId", message.FromNumberId);
                command.Parameters.AddWithValue("@ToNumberId", message.ToNumberId);

                retorno = command.ExecuteNonQuery() > 0;
                transaction.Commit();

                // Verifica se é necessário criar o contato em segundo plano
                System.Threading.Tasks.Task.Run(() =>
                {
                    CreateContact(message.FromNumberId, message.ToNumberId);
                });

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

        public override List<ChatDemo.Data.Messages>? GetMessagesByNumberId(string fromNumberId, string toNumberId)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Message, Datetime, FromNumberId, ToNumberId, Sent ");
                sql.Append("FROM Messages ");
                sql.Append("WHERE ");
                sql.Append(" (FromNumberId = @FromNumberId AND ToNumberId = @ToNumberId) ");
                sql.Append(" OR (FromNumberId = @ToNumberId AND ToNumberId = @FromNumberId) ");
                sql.Append("ORDER BY Datetime ASC ");
                sql.Append("LIMIT 250 ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@FromNumberId", fromNumberId);
                command.Parameters.AddWithValue("@ToNumberId", toNumberId);

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
                    message.Sent = message.FromNumberId == fromNumberId ? true : false;

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

        protected override void CreateContact(string fromNumberId, string toNumberId)
        {
            var connection = CriarConnection();

            try
            {
                // Cria instanciaDB de contato
                var contactDB = new ChatDemo.DAO.SQLite.ContactsDBSQLite(_connectionString);

                // Verifica se o contato (você) já existe para o outro usuário
                var contact = contactDB.GetContactByNumberIdAndMyNumberId(fromNumberId, toNumberId);

                // Se não existe, insere
                if (contact == null)
                {
                    connection.Open();

                    var userDB = new ChatDemo.DAO.SQLite.UsersDBSQLite(_connectionString);
                    var user = userDB.GetUserByNumberId(fromNumberId);

                    if (user != null)
                    {
                        contact = new ChatDemo.Data.Contacts
                        {
                            Name = null,
                            NumberId = user.NumberId,
                            MyNumberId = toNumberId,
                            WebId = user.WebId
                        };

                        contactDB.CreateContact(contact);
                    }

                }
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
    }
}
