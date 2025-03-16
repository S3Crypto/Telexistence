using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.DTOs;

namespace TelexistenceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly ICommandService _commandService;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(ICommandService commandService, ILogger<HistoryController> logger)
        {
            _commandService = commandService;
            _logger = logger;
        }

        [HttpGet("{robotId}")]
        public async Task<ActionResult<IEnumerable<CommandResponseDto>>> GetCommandHistory(
            string robotId,
            [FromQuery] int limit = 10
        )
        {
            try
            {
                var commands = await _commandService.GetCommandHistoryAsync(robotId, limit);

                var result = commands.Select(
                    command =>
                        new CommandResponseDto
                        {
                            Id = command.Id,
                            Command = command.CommandType,
                            Robot = command.RobotId,
                            Status = command.Status,
                            User = command.UserId,
                            CreatedAt = command.CreatedAt,
                            ExecutedAt = command.ExecutedAt
                        }
                );

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving command history for robot {RobotId}",
                    robotId
                );
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
