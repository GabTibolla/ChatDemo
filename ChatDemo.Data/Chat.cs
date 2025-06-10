namespace ChatDemo.Data
{
    public class Chat
    {
        public List<Contacts>? Contacts { get; set; }
        public List<Messages>? Messages { get; set; }
        public Contacts? SelectedContact { get; set; }
        public User? UserLogged { get; set; }
    }
}
