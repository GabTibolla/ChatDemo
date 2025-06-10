using System.Text;

namespace ChatDemo.DAO.SQLite
{
    public class ContactsDBSQLite : ChatDemo.DAO.ContactsDB
    {
        public ContactsDBSQLite(string connectionString) : base(connectionString)
        {
            CreateTableContacts();
        }

        public override List<ChatDemo.Data.Contacts>? GetAllContacts(string numberIdManager)
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, NumberId, MyNumberId, LastMessageId, LastMessageDate, WebId ");
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

        public override ChatDemo.Data.Contacts GetContactByWebIdAndNumberId(string webId, string myNumberId)
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            try
            {
                connection.Open();

                StringBuilder sql = new StringBuilder();
                sql.Append("SELECT Id, Name, NumberId, MyNumberId, LastMessageId, LastMessageDate, WebId ");
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

        private void CreateTableContacts()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Contacts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    NumberId TEXT,
                    MyNumberId TEXT,
                    LastMessageId INTEGER,
                    LastMessageDate TEXT,
                    WebId TEXT NOT NULL
                )";
            command.ExecuteNonQuery();
            connection.Close();
        }

    }
}
