namespace TelexistenceAPI.Core.Entities
{
    public class Robot
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public Position CurrentPosition { get; set; } = new Position();
        public string Status { get; set; } = "Idle";
        public string? CurrentTask { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Rotation { get; set; } // Degrees
    }
}
