namespace TelexistenceAPI.Core.Entities
{
    public class User
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
