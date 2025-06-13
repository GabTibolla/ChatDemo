using ChatDemo.Data;
using System.Text;
using System.Transactions;

namespace ChatDemo.DAO.SQLite
{
    public class ContactsDBSQLite : ChatDemo.DAO.ContactsDB
    {
        public ContactsDBSQLite(string connectionString) : base(connectionString)
        {
        }

        public override bool CreateContact(Contacts contact)
        {
            var connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO Contacts (Name, NumberId, MyNumberId, WebId, LastMessageId, LastMessageDate, JsonEvent) ");
                sql.Append("VALUES (@Name, @NumberId, @MyNumberId, @WebId, @LastMessageId, @LastMessageDate, @JsonEvent) ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                string? json = null;
                if(contact.LastMessage != null)
                    json = System.Text.Json.JsonSerializer.Serialize(contact.LastMessage);

                command.Parameters.AddWithValue("@Name", contact.Name ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NumberId", contact.NumberId);
                command.Parameters.AddWithValue("@MyNumberId", contact.MyNumberId);
                command.Parameters.AddWithValue("@WebId", contact.WebId);
                command.Parameters.AddWithValue("@LastMessageId", contact.LastMessageId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@LastMessageDate", contact.LastMessageDate ?? (object)DBNull.Value);
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

        public override bool UpdateContact(ChatDemo.Data.Contacts contact)
        {
            var connection = CriarConnection();
            Microsoft.Data.Sqlite.SqliteTransaction? transaction = null;
            bool retorno = false;

            try
            {
                connection.Open();
                transaction = connection.BeginTransaction();

                StringBuilder sql = new StringBuilder();
                sql.Append("UPDATE CONTACTS SET LastMessageId = @LastMessageId, LastMessageDate = @LastMessageDate, JsonEvent = @JsonEvent ");
                sql.Append("WHERE (NumberId = @NumberId AND MyNumberId = @MyNumberId) || (NumberId = @MyNumberId AND MyNumberId = @NumberId); ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                string? json = null;
                if(contact.LastMessage != null) 
                    json = System.Text.Json.JsonSerializer.Serialize(contact.LastMessage);

                command.Parameters.AddWithValue("@LastMessageId", contact.LastMessageId);
                command.Parameters.AddWithValue("@LastMessageDate", contact.LastMessageDate);
                command.Parameters.AddWithValue("@NumberId", contact.NumberId);
                command.Parameters.AddWithValue("@MyNumberId", contact.MyNumberId);
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

        public override List<ChatDemo.Data.Contacts>? GetAllContacts(string numberIdManager)
        {
            var connection = CriarConnection();
            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, NumberId, MyNumberId, LastMessageId, LastMessageDate, WebId, JsonEvent ");
                sql.Append("FROM Contacts ");
                sql.Append("WHERE MyNumberId = @myNumberId ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@myNumberId", numberIdManager);
                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                List<ChatDemo.Data.Contacts>? contacts = new List<ChatDemo.Data.Contacts>();
                while (reader.Read())
                {
                    var contact = new ChatDemo.Data.Contacts();
                    contact.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    contact.Name = reader.IsDBNull(1) ? null : reader.GetString(1);
                    contact.NumberId = reader.IsDBNull(2) ? null : reader.GetString(2);
                    contact.MyNumberId = reader.IsDBNull(3) ? null : reader.GetString(3);
                    contact.LastMessageId = reader.IsDBNull(4) ? null : reader.GetInt32(4);
                    contact.LastMessageDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5));
                    contact.WebId = reader.IsDBNull(6) ? null : reader.GetString(6);

                    // Se a última mensagem for nula, não atribui
                    if (!reader.IsDBNull(7))
                    {
                        string json = reader.GetString(7);
                        contact.LastMessage = System.Text.Json.JsonSerializer.Deserialize<ChatDemo.Data.Message>(json);
                    }

                    contacts.Add(contact);
                }

                return contacts;
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

        public override ChatDemo.Data.Contacts? GetContactByNumberIdAndMyNumberId(string numberId, string myNumberId)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, NumberId, MyNumberId, LastMessageId, LastMessageDate, WebId, JsonEvent ");
                sql.Append("FROM Contacts ");
                sql.Append("WHERE NumberId = @NumberId AND MyNumberId = @myNumberId ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@NumberId", numberId);
                command.Parameters.AddWithValue("@myNumberId", myNumberId);

                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                ChatDemo.Data.Contacts? contact = null;
                if (reader.Read())
                {
                    contact = new ChatDemo.Data.Contacts();
                    contact.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    contact.Name = reader.IsDBNull(1) ? null : reader.GetString(1);
                    contact.NumberId = reader.IsDBNull(2) ? null : reader.GetString(2);
                    contact.MyNumberId = reader.IsDBNull(3) ? null : reader.GetString(3);
                    contact.LastMessageId = reader.IsDBNull(4) ? null : reader.GetInt32(4);
                    contact.LastMessageDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5));
                    contact.WebId = reader.IsDBNull(6) ? null : reader.GetString(6);

                    // Se a última mensagem for nula, não atribui
                    if (!reader.IsDBNull(7))
                    {
                        string json = reader.GetString(7);
                        contact.LastMessage = System.Text.Json.JsonSerializer.Deserialize<ChatDemo.Data.Message>(json);
                    }

                }

                return contact;
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

        public override ChatDemo.Data.Contacts GetContactByWebIdAndNumberId(string webId, string myNumberId)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, NumberId, MyNumberId, LastMessageId, LastMessageDate, WebId, JsonEvent ");
                sql.Append("FROM Contacts ");
                sql.Append("WHERE WebId = @webId AND MyNumberId = @myNumberId ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@webId", webId);
                command.Parameters.AddWithValue("@myNumberId", myNumberId);
                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                ChatDemo.Data.Contacts? contact = null;
                if (reader.Read())
                {
                    contact = new ChatDemo.Data.Contacts();
                    contact.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    contact.Name = reader.IsDBNull(1) ? null : reader.GetString(1);
                    contact.NumberId = reader.IsDBNull(2) ? null : reader.GetString(2);
                    contact.MyNumberId = reader.IsDBNull(3) ? null : reader.GetString(3);
                    contact.LastMessageId = reader.IsDBNull(4) ? null : reader.GetInt32(4);
                    contact.LastMessageDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5));
                    contact.WebId = reader.IsDBNull(6) ? null : reader.GetString(6);

                    // Se a última mensagem for nula, não atribui
                    if (!reader.IsDBNull(7))
                    {
                        string json = reader.GetString(7);
                        contact.LastMessage = System.Text.Json.JsonSerializer.Deserialize<ChatDemo.Data.Message>(json);
                    }
                }

                return contact;
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
