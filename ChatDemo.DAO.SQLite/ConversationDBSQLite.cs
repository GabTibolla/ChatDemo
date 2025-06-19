using System.Text;

namespace ChatDemo.DAO.SQLite
{
    public class ConversationDBSQLite : ChatDemo.DAO.ConversationsDB
    {
        public ConversationDBSQLite(string connectionString) : base(connectionString)
        {
        }

        public override bool CreateConversation(ChatDemo.Data.Conversation conversation)
        {
            Microsoft.Data.Sqlite.SqliteConnection connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Conversations (Id, ContactNumberId, OwnerNumberId) ");
                sql.Append("VALUES (@Id, @ContactNumberId, @OwnerNumberId);");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();
                command.Parameters.AddWithValue("@Id", conversation.Id);
                command.Parameters.AddWithValue("@ContactNumberId", conversation.ContactNumberId);
                command.Parameters.AddWithValue("@OwnerNumberId", conversation.OwnerNumberId);

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
        public override List<ChatDemo.Data.Conversation>? GetConversations(string OwnerNumberId)
        {
            Microsoft.Data.Sqlite.SqliteConnection connection = CriarConnection();
            List<ChatDemo.Data.Conversation>? retorno = null;

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();

                sql.Append("SELECT Id, ContactNumberId, OwnerNumberId, CreatedAt, JsonEvent ");
                sql.Append("FROM Conversations ");
                sql.Append("WHERE OwnerNumberId = @OwnerNumberId");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();
                command.Parameters.AddWithValue("@OwnerNumberId", OwnerNumberId);

                var reader = command.ExecuteReader();

                retorno = new List<ChatDemo.Data.Conversation>();
                while (reader.Read())
                {
                    var conversationData = new ChatDemo.Data.Conversation
                    {
                        Id = reader.GetString(0),
                        ContactNumberId = reader.GetString(1),
                        OwnerNumberId = reader.GetString(2),
                        CreatedAt = reader.GetString(3)
                    };

                    // Se a última mensagem for nula, não atribui
                    if (!reader.IsDBNull(4))
                    {
                        string json = reader.GetString(4);
                        conversationData.LastMessage = System.Text.Json.JsonSerializer.Deserialize<ChatDemo.Data.Message>(json);
                    }

                    retorno.Add(conversationData);
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

            return retorno;
        }
        public override ChatDemo.Data.Conversation? GetConversation(string conversationId)
        {
            Microsoft.Data.Sqlite.SqliteConnection connection = CriarConnection();
            ChatDemo.Data.Conversation? retorno = null;

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();

                sql.Append("SELECT Id, ContactNumberId, OwnerNumberId, CreatedAt, JsonEvent ");
                sql.Append("FROM Conversations ");
                sql.Append("WHERE Id = @Id;");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();
                command.Parameters.AddWithValue("@Id", conversationId);

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    retorno = new ChatDemo.Data.Conversation
                    {
                        Id = reader.GetString(0),
                        ContactNumberId = reader.GetString(1),
                        OwnerNumberId = reader.GetString(2),
                        CreatedAt = reader.GetString(3)
                    };

                    // Se a última mensagem for nula, não atribui
                    if (!reader.IsDBNull(4))
                    {
                        string json = reader.GetString(4);
                        retorno.LastMessage = System.Text.Json.JsonSerializer.Deserialize<ChatDemo.Data.Message>(json);
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

            return retorno;
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

        public override bool UpdateConversation(ChatDemo.Data.Conversation conversation)
        {
            var connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            string conversationId = ChatDemo.Business.Helper.GerarConversationId(conversation.ContactNumberId, conversation.OwnerNumberId);

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("UPDATE Conversations SET JsonEvent = @JsonEvent ");
                sql.Append("WHERE Id = @Id");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                string? json = null;
                if (conversation.LastMessage != null)
                    json = System.Text.Json.JsonSerializer.Serialize(conversation.LastMessage);

                command.Parameters.AddWithValue("@Id", conversationId);
                command.Parameters.AddWithValue("@JsonEvent", json ?? (object)DBNull.Value);
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
    }
}
