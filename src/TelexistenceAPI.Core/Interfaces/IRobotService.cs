using TelexistenceAPI.Core.Entities;

namespace TelexistenceAPI.Core.Interfaces
{
    public interface IRobotService
    {
        Task<Robot?> GetRobotAsync(string id);
        Task<IEnumerable<Robot>> GetAllRobotsAsync();
        Task<Robot> UpdateRobotStatusAsync(string id, string status, string? currentTask = null);
        Task<Robot> UpdateRobotPositionAsync(string id, Position position);
    }
}
