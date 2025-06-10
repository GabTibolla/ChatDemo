using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatDemo.DAO
{
    public class ContactsDB
    {
        protected readonly string _connectionString;

        public ContactsDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static ChatDemo.DAO.ContactsDB? Create(string className, string projectName, string connectionString)
        {
            var type = Type.GetType($"{className}, {projectName}", true);
            var instance = Activator.CreateInstance(type, connectionString);

            return (ChatDemo.DAO.ContactsDB?)instance;
        }

        public virtual List<ChatDemo.Data.Contacts>? GetAllContacts(string myNumberId)
        {
            throw new NotImplementedException();
        }

        public virtual ChatDemo.Data.Contacts GetContact(string webId)
        {
            throw new NotImplementedException();
        }

        public virtual ChatDemo.Data.Contacts GetContactByNumberId(string numberId, string myNumberId)
        {
            throw new NotImplementedException();
        }
    }
}
