namespace SqliteDapperDemo.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public DateTime CreateTime { get; set; }
    }
} 