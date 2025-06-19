using ChatDemo.Data;
using System.Text;

namespace ChatDemo.DAO.SQLite
{
    public class MessagesDBSQLite : ChatDemo.DAO.MessagesDB
    {
        public MessagesDBSQLite(string connectionString) : base(connectionString)
        {
        }

        public override bool AddMessage(ChatDemo.Data.Message message, string ContactNumberId)
        {
            var connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Messages (ConversationId, SenderNumberId, WebId, Text, DateTime, Status) ");
                sql.Append("VALUES (@ConversationId, @SenderNumberId, @WebId, @Text, @DateTime, @Status); ");
                sql.Append("SELECT last_insert_rowid();");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();
                command.Parameters.AddWithValue("@ConversationId", message.ConversationId);
                command.Parameters.AddWithValue("@SenderNumberId", message.SenderNumberId);
                command.Parameters.AddWithValue("@WebId", message.WebId);
                command.Parameters.AddWithValue("@Text", message.Text);
                command.Parameters.AddWithValue("@DateTime", message.Datetime);
                command.Parameters.AddWithValue("@Status", (int) message.Status);

                int idMessage = Convert.ToInt32(command.ExecuteScalar());

                if (idMessage > 0)
                {
                    retorno = true;

                    // Verifica se é necessário criar o contato em segundo plano
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        message.Id = idMessage;
                        CreateContactAndUpdateConversation(message, ContactNumberId);
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

        public override bool UpdateStatusMessageToRead(string conversationId, string SenderNumberId)
        {
            var connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("UPDATE Messages SET Status = 2 ");
                sql.Append("WHERE ConversationId = @ConversationId AND SenderNumberId <> @SenderNumberId ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();
                command.Parameters.AddWithValue("@ConversationId", conversationId);
                command.Parameters.AddWithValue("@SenderNumberId", SenderNumberId);

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

        public override List<ChatDemo.Data.Message>? GetMessages(string conversationId)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, SenderNumberId, Text, DateTime, Status, WebId ");
                sql.Append("FROM Messages ");
                sql.Append("WHERE ConversationId = @ConversationId ");
                sql.Append("ORDER BY Datetime ASC ");
                sql.Append("LIMIT 250 ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@ConversationId", conversationId);

                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                List<ChatDemo.Data.Message>? messages = new List<Data.Message>();
                while (reader.Read())
                {
                    var message = new ChatDemo.Data.Message();
                    message.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    message.SenderNumberId = reader.IsDBNull(1) ? null : reader.GetString(1);
                    message.Text = reader.IsDBNull(2) ? null : reader.GetString(2);
                    message.Datetime = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                    message.Status = (ChatDemo.Data.Message.StatusMessage) reader.GetInt32(4);
                    message.WebId = reader.IsDBNull(5) ? null : reader.GetString(5);
                    message.ConversationId = conversationId;

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

        protected override void CreateContactAndUpdateConversation(ChatDemo.Data.Message message, string ContactNumberId)
        {
            var connection = CriarConnection();

            try
            {
                // Cria instanciaDB de contato
                var contactDB = new ChatDemo.DAO.SQLite.ContactsDBSQLite(_connectionString);

                // Verifica se o contato (você) já existe para o outro usuário (lógica inversa)
                var contact = contactDB.GetContactByOwnerAndContactNumberId(message.SenderNumberId, ContactNumberId);

                // Se não existe, insere
                if (contact == null)
                {
                    connection.Open();

                    var userDB = new ChatDemo.DAO.SQLite.UsersDBSQLite(_connectionString);
                    var user = userDB.GetUserByNumberId(message.SenderNumberId);

                    if (user != null)
                    {
                        contact = new ChatDemo.Data.Contacts
                        {
                            Alias = null,
                            ContactNumberId = user.NumberId,
                            OwnerNumberId = ContactNumberId, // (Agora o contato é o dono)
                        };

                        contactDB.CreateContact(contact);
                    }
                }

                // Atualiza a conversa com o contato
                ChatDemo.Data.Conversation? conversation = null;
                var conversationDB = new ChatDemo.DAO.SQLite.ConversationDBSQLite(_connectionString);

                conversation = conversationDB.GetConversation(message.ConversationId);
                if (conversation != null)
                {
                    conversation.LastMessage = message;
                    conversationDB.UpdateConversation(conversation);
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
