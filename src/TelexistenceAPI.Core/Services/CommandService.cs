// TelexistenceAPI.Core/Services/CommandService.cs
using TelexistenceAPI.Core.Entities;
using TelexistenceAPI.Core.Interfaces;

namespace TelexistenceAPI.Core.Services
{
    public class CommandService : ICommandService
    {
        private readonly ICommandRepository _commandRepository;
        private readonly IRobotRepository _robotRepository;
        private readonly ILogger<CommandService> _logger;

        public CommandService(
            ICommandRepository commandRepository,
            IRobotRepository robotRepository,
            ILogger<CommandService> logger
        )
        {
            _commandRepository = commandRepository;
            _robotRepository = robotRepository;
            _logger = logger;
        }

        public async Task<Command> CreateCommandAsync(
            string commandType,
            string robotId,
            string userId,
            Dictionary<string, object>? parameters = null
        )
        {
            var robot = await _robotRepository.GetAsync(robotId);
            if (robot == null)
            {
                throw new KeyNotFoundException($"Robot with ID {robotId} not found");
            }

            // Validate command
            ValidateCommand(commandType, parameters);

            var command = new Command
            {
                Id = Guid.NewGuid().ToString(),
                CommandType = commandType,
                Parameters = parameters ?? new Dictionary<string, object>(),
                RobotId = robotId,
                UserId = userId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Creating command {CommandType} for robot {RobotId}",
                commandType,
                robotId
            );
            return await _commandRepository.CreateAsync(command);
        }

        public async Task<Command?> GetCommandAsync(string id)
        {
            return await _commandRepository.GetAsync(id);
        }

        public async Task<Command> UpdateCommandAsync(
            string id,
            string commandType,
            Dictionary<string, object>? parameters = null
        )
        {
            var command = await _commandRepository.GetAsync(id);
            if (command == null)
            {
                throw new KeyNotFoundException($"Command with ID {id} not found");
            }

            if (command.Status != "Pending")
            {
                throw new InvalidOperationException("Only pending commands can be updated");
            }

            // Validate command
            ValidateCommand(commandType, parameters);

            command.CommandType = commandType;
            command.Parameters = parameters ?? new Dictionary<string, object>();

            _logger.LogInformation("Updating command {CommandId}", id);
            return await _commandRepository.UpdateAsync(command);
        }

        public async Task<IEnumerable<Command>> GetCommandHistoryAsync(
            string robotId,
            int limit = 10
        )
        {
            var robot = await _robotRepository.GetAsync(robotId);
            if (robot == null)
            {
                throw new KeyNotFoundException($"Robot with ID {robotId} not found");
            }

            return await _commandRepository.GetHistoryAsync(robotId, limit);
        }

        public async Task<bool> ExecuteCommandAsync(string id)
        {
            var command = await _commandRepository.GetAsync(id);
            if (command == null)
            {
                throw new KeyNotFoundException($"Command with ID {id} not found");
            }

            if (command.Status != "Pending")
            {
                throw new InvalidOperationException(
                    $"Command is already in {command.Status} state"
                );
            }

            var robot = await _robotRepository.GetAsync(command.RobotId);
            if (robot == null)
            {
                throw new KeyNotFoundException($"Robot with ID {command.RobotId} not found");
            }

            try
            {
                // Update command status
                command.Status = "Executing";
                await _commandRepository.UpdateAsync(command);

                // Execute command based on its type
                switch (command.CommandType.ToLower())
                {
                    case "move":
                        await ExecuteMoveCommand(robot, command);
                        break;
                    case "rotate":
                        await ExecuteRotateCommand(robot, command);
                        break;
                    default:
                        throw new NotImplementedException(
                            $"Command type {command.CommandType} not implemented"
                        );
                }

                // Mark command as completed
                command.Status = "Completed";
                command.ExecutedAt = DateTime.UtcNow;
                await _commandRepository.UpdateAsync(command);

                _logger.LogInformation("Command {CommandId} executed successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                // Mark command as failed
                command.Status = "Failed";
                await _commandRepository.UpdateAsync(command);

                _logger.LogError(ex, "Failed to execute command {CommandId}", id);
                return false;
            }
        }

        private void ValidateCommand(string commandType, Dictionary<string, object>? parameters)
        {
            switch (commandType.ToLower())
            {
                case "move":
                    // Validate move
                    if (parameters == null || !parameters.ContainsKey("direction"))
                    {
                        throw new ArgumentException(
                            "Move command requires a 'direction' parameter"
                        );
                    }
                    break;
                case "rotate":
                    if (parameters == null || !parameters.ContainsKey("degrees"))
                    {
                        throw new ArgumentException(
                            "Rotate command requires a 'degrees' parameter"
                        );
                    }
                    break;
                default:
                    throw new ArgumentException($"Unsupported command type: {commandType}");
            }
        }

        private async Task ExecuteMoveCommand(Robot robot, Command command)
        {
            string direction = command.Parameters["direction"].ToString()?.ToLower() ?? "forward";
            double distance = 1.0; // Default distance

            if (
                command.Parameters.ContainsKey("distance")
                && command.Parameters["distance"] is double distanceValue
            )
            {
                distance = distanceValue;
            }

            // Update robot position based on direction
            switch (direction)
            {
                case "forward":
                    robot.CurrentPosition.X +=
                        Math.Cos(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    robot.CurrentPosition.Y +=
                        Math.Sin(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    break;
                case "backward":
                    robot.CurrentPosition.X -=
                        Math.Cos(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    robot.CurrentPosition.Y -=
                        Math.Sin(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    break;
                case "left":
                    robot.CurrentPosition.X -=
                        Math.Sin(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    robot.CurrentPosition.Y +=
                        Math.Cos(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    break;
                case "right":
                    robot.CurrentPosition.X +=
                        Math.Sin(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    robot.CurrentPosition.Y -=
                        Math.Cos(robot.CurrentPosition.Rotation * Math.PI / 180) * distance;
                    break;
                default:
                    throw new ArgumentException($"Unsupported direction: {direction}");
            }

            robot.Status = "Moving";
            robot.CurrentTask = $"Moving {direction}";
            robot.LastUpdated = DateTime.UtcNow;

            await _robotRepository.UpdateAsync(robot);
        }

        private async Task ExecuteRotateCommand(Robot robot, Command command)
        {
            if (
                !command.Parameters.ContainsKey("degrees")
                || !(
                    command.Parameters["degrees"] is double || command.Parameters["degrees"] is int
                )
            )
            {
                throw new ArgumentException("Rotate command requires a valid 'degrees' parameter");
            }

            double degrees = Convert.ToDouble(command.Parameters["degrees"]);

            // Update robot rotation
            robot.CurrentPosition.Rotation = (robot.CurrentPosition.Rotation + degrees) % 360;
            if (robot.CurrentPosition.Rotation < 0)
            {
                robot.CurrentPosition.Rotation += 360;
            }

            robot.Status = "Rotating";
            robot.CurrentTask = $"Rotating {degrees} degrees";
            robot.LastUpdated = DateTime.UtcNow;

            await _robotRepository.UpdateAsync(robot);
        }
    }
}
