namespace TelexistenceAPI.DTOs
{
    public class RobotStatusDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public PositionDto Position { get; set; } = new PositionDto();
        public string Status { get; set; } = null!;
        public string? CurrentTask { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PositionDto
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Rotation { get; set; }
    }
}
