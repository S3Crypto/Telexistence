namespace TelexistenceAPI.Core.Entities
{
    public class Command
    {
        public string Id { get; set; } = null!;
        public string CommandType { get; set; } = null!; // "Move", "Rotate", etc.
        public Dictionary<string, object> Parameters { get; set; } =
            new Dictionary<string, object>();
        public string RobotId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Status { get; set; } = "Pending"; // Pending, Executing, Completed, Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExecutedAt { get; set; }
    }
}
