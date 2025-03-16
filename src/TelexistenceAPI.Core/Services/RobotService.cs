using Microsoft.Extensions.Logging;
using TelexistenceAPI.Core.Entities;
using TelexistenceAPI.Core.Interfaces;

namespace TelexistenceAPI.Core.Services
{
    public class RobotService : IRobotService
    {
        private readonly IRobotRepository _robotRepository;
        private readonly ILogger<RobotService> _logger;

        public RobotService(IRobotRepository robotRepository, ILogger<RobotService> logger)
        {
            _robotRepository = robotRepository;
            _logger = logger;
        }

        public async Task<Robot?> GetRobotAsync(string id)
        {
            return await _robotRepository.GetAsync(id);
        }

        public async Task<IEnumerable<Robot>> GetAllRobotsAsync()
        {
            return await _robotRepository.GetAllAsync();
        }

        public async Task<Robot> UpdateRobotStatusAsync(
            string id,
            string status,
            string? currentTask = null
        )
        {
            var robot = await _robotRepository.GetAsync(id);
            if (robot == null)
            {
                throw new KeyNotFoundException($"Robot with ID {id} not found");
            }

            robot.Status = status;
            robot.CurrentTask = currentTask;
            robot.LastUpdated = DateTime.UtcNow;

            _logger.LogInformation("Updating robot {RobotId} status to {Status}", id, status);
            return await _robotRepository.UpdateAsync(robot);
        }

        public async Task<Robot> UpdateRobotPositionAsync(string id, Position position)
        {
            var robot = await _robotRepository.GetAsync(id);
            if (robot == null)
            {
                throw new KeyNotFoundException($"Robot with ID {id} not found");
            }

            robot.CurrentPosition = position;
            robot.LastUpdated = DateTime.UtcNow;

            _logger.LogInformation("Updating robot {RobotId} position", id);
            return await _robotRepository.UpdateAsync(robot);
        }
    }
}
