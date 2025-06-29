﻿namespace ChatDemo.DAO
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
        public virtual bool CreateContact(ChatDemo.Data.Contacts contact)
        {
            throw new NotImplementedException();
        }
        public virtual bool UpdateContact(ChatDemo.Data.Contacts contact)
        {
            throw new NotImplementedException();
        }
        public virtual bool UpdateUnreadMessages(int unreadMessages, string ContactNumberId, string OwnerNumberId)
        {
            throw new NotImplementedException();
        }
        public virtual List<ChatDemo.Data.Contacts>? GetContacts(string OwnerNumberId)
        {
            throw new NotImplementedException();
        }
        public virtual ChatDemo.Data.Contacts? GetContactByOwnerAndContactNumberId(string ContactNumberId, string OwnerNumberId)
        {
            throw new NotImplementedException();
        }
        public virtual ChatDemo.Data.Contacts GetContact(string webId)
        {
            throw new NotImplementedException();
        }
        public virtual ChatDemo.Data.Contacts GetContactByWebIdAndNumberId(string webId, string OwnerNumberId)
        {
            throw new NotImplementedException();
        }
    }
}
