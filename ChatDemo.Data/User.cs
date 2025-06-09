namespace ChatDemo.Data
{
    public class User
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Cpf { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? Password { get; set; }
    }
}
