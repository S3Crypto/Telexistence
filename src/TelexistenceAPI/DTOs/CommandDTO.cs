namespace TelexistenceAPI.DTOs
{
    public class CommandRequestDto
    {
        public string Command { get; set; } = null!;
        public string Robot { get; set; } = null!;
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class CommandResponseDto
    {
        public string Id { get; set; } = null!;
        public string Command { get; set; } = null!;
        public string Robot { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExecutedAt { get; set; }
    }

    public class CommandUpdateDto
    {
        public string Id { get; set; } = null!;
        public string Command { get; set; } = null!;
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
