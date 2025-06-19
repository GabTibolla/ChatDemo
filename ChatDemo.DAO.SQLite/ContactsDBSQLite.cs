using ChatDemo.Data;
using System.Text;

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
                sql.Append("INSERT INTO Contacts (Alias, OwnerNumberId, ContactNumberId) ");
                sql.Append("VALUES (@Alias, @OwnerNumberId, @ContactNumberId) ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@Alias", contact.Alias ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@OwnerNumberId", contact.OwnerNumberId);
                command.Parameters.AddWithValue("@ContactNumberId", contact.ContactNumberId);
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

        public override List<ChatDemo.Data.Contacts>? GetContacts(string OwnerNumberId)
        {
            var connection = CriarConnection();
            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Alias, OwnerNumberId, ContactNumberId ");
                sql.Append("FROM Contacts ");
                sql.Append("WHERE OwnerNumberId = @OwnerNumberId");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@OwnerNumberId", OwnerNumberId);
                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                List<ChatDemo.Data.Contacts>? contacts = new List<ChatDemo.Data.Contacts>();
                while (reader.Read())
                {
                    var contact = new ChatDemo.Data.Contacts();
                    contact.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    contact.Alias = reader.IsDBNull(1) ? null : reader.GetString(1);
                    contact.OwnerNumberId = reader.IsDBNull(2) ? null : reader.GetString(2);
                    contact.ContactNumberId = reader.IsDBNull(3) ? null : reader.GetString(3);

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

        public override ChatDemo.Data.Contacts? GetContactByNumberIdAndMyNumberId(string ContactNumberId, string OwnerNumberId)
        {
            var connection = CriarConnection();

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Alias, OwnerNumberId, ContactNumberId ");
                sql.Append("FROM Contacts ");
                sql.Append("WHERE ContactNumberId = @ContactNumberId AND OwnerNumberId = @OwnerNumberId ");

                var command = connection.CreateCommand();
                command.CommandText = sql.ToString();

                command.Parameters.AddWithValue("@ContactNumberId", ContactNumberId);
                command.Parameters.AddWithValue("@OwnerNumberId", OwnerNumberId);

                Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

                ChatDemo.Data.Contacts? contact = null;
                if (reader.Read())
                {
                    contact = new ChatDemo.Data.Contacts();
                    contact.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    contact.Alias = reader.IsDBNull(1) ? null : reader.GetString(1);
                    contact.OwnerNumberId = reader.IsDBNull(2) ? null : reader.GetString(2);
                    contact.ContactNumberId = reader.IsDBNull(3) ? null : reader.GetString(3);
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

        //public override ChatDemo.Data.Contacts GetContactByWebIdAndNumberId(string webId, string myNumberId)
        //{
        //    var connection = CriarConnection();

        //    try
        //    {
        //        connection.Open();

        //        StringBuilder sql = new StringBuilder();
        //        sql.Append("SELECT Id, Name, NumberId, MyNumberId, LastMessageId, LastMessageDate, WebId, JsonEvent ");
        //        sql.Append("FROM Contacts ");
        //        sql.Append("WHERE WebId = @webId AND MyNumberId = @myNumberId ");

        //        var command = connection.CreateCommand();
        //        command.CommandText = sql.ToString();

        //        command.Parameters.AddWithValue("@webId", webId);
        //        command.Parameters.AddWithValue("@myNumberId", myNumberId);
        //        Microsoft.Data.Sqlite.SqliteDataReader reader = command.ExecuteReader();

        //        ChatDemo.Data.Contacts? contact = null;
        //        if (reader.Read())
        //        {
        //            contact = new ChatDemo.Data.Contacts();
        //            contact.Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
        //            contact.Name = reader.IsDBNull(1) ? null : reader.GetString(1);
        //            contact.NumberId = reader.IsDBNull(2) ? null : reader.GetString(2);
        //            contact.MyNumberId = reader.IsDBNull(3) ? null : reader.GetString(3);
        //            contact.LastMessageId = reader.IsDBNull(4) ? null : reader.GetInt32(4);
        //            contact.LastMessageDate = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5));
        //            contact.WebIdContact = reader.IsDBNull(6) ? null : reader.GetString(6);

        //            // Se a última mensagem for nula, não atribui
        //            if (!reader.IsDBNull(7))
        //            {
        //                string json = reader.GetString(7);
        //                contact.LastMessage = System.Text.Json.JsonSerializer.Deserialize<ChatDemo.Data.Message>(json);
        //            }
        //        }

        //        return contact;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        connection.Close();
        //    }
        //}

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
