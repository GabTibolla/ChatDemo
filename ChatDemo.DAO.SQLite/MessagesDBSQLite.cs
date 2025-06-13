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

        public override bool AddMessage(ChatDemo.Data.Message message)
        {
            var connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Messages (Text, DateTime, FromNumberId, ToNumberId) ");
                sql.Append("VALUES (@text, @Datetime, @FromNumberId, @ToNumberId); ");
                sql.Append("SELECT last_insert_rowid();");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();
                command.Parameters.AddWithValue("@text", message.Text);
                command.Parameters.AddWithValue("@Datetime", message.Datetime);
                command.Parameters.AddWithValue("@FromNumberId", message.FromNumberId);
                command.Parameters.AddWithValue("@ToNumberId", message.ToNumberId);


                int idMessage = Convert.ToInt32(command.ExecuteScalar());

                if (idMessage > 0)
                {
                    retorno = true;

                    // Verifica se é necessário criar o contato em segundo plano
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        message.Id = idMessage;
                        CreateContact(message.FromNumberId, message.ToNumberId, message);
                    });

                    transaction.Commit();
                }

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

        public override List<ChatDemo.Data.Message>? GetMessagesByNumberId(string fromNumberId, string toNumberId)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Text, Datetime, FromNumberId, ToNumberId ");
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

                List<ChatDemo.Data.Message>? messages = new List<Data.Message>();
                while (reader.Read())
                {
                    var message = new ChatDemo.Data.Message();
                    message.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    message.Text = reader.IsDBNull(1) ? null : reader.GetString(1);
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

        protected override void CreateContact(string fromNumberId, string toNumberId, ChatDemo.Data.Message message)
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
                            WebId = user.WebId,
                            LastMessageId = message.Id,
                            LastMessageDate = message.Datetime,
                            LastMessage = message,
                        };

                        contactDB.CreateContact(contact);
                    }
                    else
                    {
                        // Erro, pois o contato nem existe, logo não vai atualizar nada e nem ninguém.
                        return;
                    }
                }

                // Atualiza os dois contatos com a mesma mensagem e dataHora
                contact.LastMessageId = message.Id;
                contact.LastMessageDate = message.Datetime;
                contact.LastMessage = message;

                connection.Open();
                contactDB.UpdateContact(contact);

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
