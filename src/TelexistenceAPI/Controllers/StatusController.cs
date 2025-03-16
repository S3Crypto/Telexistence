using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.DTOs;

namespace TelexistenceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatusController : ControllerBase
    {
        private readonly IRobotService _robotService;
        private readonly ILogger<StatusController> _logger;

        public StatusController(IRobotService robotService, ILogger<StatusController> logger)
        {
            _robotService = robotService;
            _logger = logger;
        }

        [HttpGet("{robotId}")]
        public async Task<ActionResult<RobotStatusDto>> GetRobotStatus(string robotId)
        {
            try
            {
                var robot = await _robotService.GetRobotAsync(robotId);
                if (robot == null)
                {
                    return NotFound();
                }

                return Ok(
                    new RobotStatusDto
                    {
                        Id = robot.Id,
                        Name = robot.Name,
                        Position = new PositionDto
                        {
                            X = robot.CurrentPosition.X,
                            Y = robot.CurrentPosition.Y,
                            Z = robot.CurrentPosition.Z,
                            Rotation = robot.CurrentPosition.Rotation
                        },
                        Status = robot.Status,
                        CurrentTask = robot.CurrentTask,
                        LastUpdated = robot.LastUpdated
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving status for robot {RobotId}", robotId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RobotStatusDto>>> GetAllRobotStatuses()
        {
            try
            {
                var robots = await _robotService.GetAllRobotsAsync();

                var result = robots.Select(
                    robot =>
                        new RobotStatusDto
                        {
                            Id = robot.Id,
                            Name = robot.Name,
                            Position = new PositionDto
                            {
                                X = robot.CurrentPosition.X,
                                Y = robot.CurrentPosition.Y,
                                Z = robot.CurrentPosition.Z,
                                Rotation = robot.CurrentPosition.Rotation
                            },
                            Status = robot.Status,
                            CurrentTask = robot.CurrentTask,
                            LastUpdated = robot.LastUpdated
                        }
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all robot statuses");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
