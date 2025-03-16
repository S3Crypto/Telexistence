using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.DTOs;
using System.Security.Claims;

namespace TelexistenceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommandController : ControllerBase
    {
        private readonly ICommandService _commandService;
        private readonly ILogger<CommandController> _logger;

        public CommandController(ICommandService commandService, ILogger<CommandController> logger)
        {
            _commandService = commandService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<CommandResponseDto>> CreateCommand(
            [FromBody] CommandRequestDto request
        )
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var command = await _commandService.CreateCommandAsync(
                    request.Command,
                    request.Robot,
                    userId,
                    request.Parameters
                );

                // Execute the command
                await _commandService.ExecuteCommandAsync(command.Id);

                // Fetch the updated command
                var updatedCommand = await _commandService.GetCommandAsync(command.Id);
                if (updatedCommand == null)
                {
                    return NotFound();
                }

                return Ok(
                    new CommandResponseDto
                    {
                        Id = updatedCommand.Id,
                        Command = updatedCommand.CommandType,
                        Robot = updatedCommand.RobotId,
                        Status = updatedCommand.Status,
                        User = updatedCommand.UserId,
                        CreatedAt = updatedCommand.CreatedAt,
                        ExecutedAt = updatedCommand.ExecutedAt
                    }
                );
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating command");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommandResponseDto>> GetCommand(string id)
        {
            try
            {
                var command = await _commandService.GetCommandAsync(id);
                if (command == null)
                {
                    return NotFound();
                }

                return Ok(
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving command {CommandId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CommandResponseDto>> UpdateCommand(
            string id,
            [FromBody] CommandUpdateDto request
        )
        {
            try
            {
                var command = await _commandService.UpdateCommandAsync(
                    id,
                    request.Command,
                    request.Parameters
                );

                return Ok(
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
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating command {CommandId}", id);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}
