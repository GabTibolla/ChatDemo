namespace ChatDemo.Data
{
    public class User
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Cpf { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? Password { get; set; }
        public string? WebId { get; set; }
        public string? NumberId { get; set; }
    }
}
