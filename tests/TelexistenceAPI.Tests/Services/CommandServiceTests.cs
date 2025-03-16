using Microsoft.Extensions.Logging;
using Moq;
using TelexistenceAPI.Core.Entities;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.Core.Services;
using Xunit;

namespace TelexistenceAPI.Tests.Services
{
    public class CommandServiceTests
    {
        private readonly Mock<ICommandRepository> _mockCommandRepository;
        private readonly Mock<IRobotRepository> _mockRobotRepository;
        private readonly Mock<ILogger<CommandService>> _mockLogger;
        private readonly CommandService _commandService;

        public CommandServiceTests()
        {
            _mockCommandRepository = new Mock<ICommandRepository>();
            _mockRobotRepository = new Mock<IRobotRepository>();
            _mockLogger = new Mock<ILogger<CommandService>>();
            _commandService = new CommandService(
                _mockCommandRepository.Object,
                _mockRobotRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateCommandAsync_ValidParameters_ReturnsNewCommand()
        {
            // Arrange
            var robotId = "1";
            var userId = "user1";
            var commandType = "move";
            var parameters = new Dictionary<string, object> { { "direction", "forward" } };

            var robot = new Robot
            {
                Id = robotId,
                Name = "Robot1",
                Status = "Idle"
            };

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync(robot);

            _mockCommandRepository
                .Setup(r => r.CreateAsync(It.IsAny<Command>()))
                .ReturnsAsync((Command c) => c);

            // Act
            var result = await _commandService.CreateCommandAsync(
                commandType,
                robotId,
                userId,
                parameters
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commandType, result.CommandType);
            Assert.Equal(robotId, result.RobotId);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Pending", result.Status);
            Assert.NotNull(result.Parameters);
            Assert.True(result.Parameters.ContainsKey("direction"));
            Assert.Equal("forward", result.Parameters["direction"]);

            _mockCommandRepository.Verify(r => r.CreateAsync(It.IsAny<Command>()), Times.Once);
        }

        [Fact]
        public async Task CreateCommandAsync_RobotNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var robotId = "nonexistent";
            var userId = "user1";
            var commandType = "move";
            var parameters = new Dictionary<string, object> { { "direction", "forward" } };

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync((Robot)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _commandService.CreateCommandAsync(commandType, robotId, userId, parameters)
            );

            _mockCommandRepository.Verify(r => r.CreateAsync(It.IsAny<Command>()), Times.Never);
        }

        [Fact]
        public async Task CreateCommandAsync_InvalidCommandType_ThrowsArgumentException()
        {
            // Arrange
            var robotId = "1";
            var userId = "user1";
            var commandType = "invalidCommand";
            var parameters = new Dictionary<string, object> { { "param", "value" } };

            var robot = new Robot
            {
                Id = robotId,
                Name = "Robot1",
                Status = "Idle"
            };

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync(robot);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _commandService.CreateCommandAsync(commandType, robotId, userId, parameters)
            );

            _mockCommandRepository.Verify(r => r.CreateAsync(It.IsAny<Command>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteCommandAsync_MoveCommand_UpdatesRobotPosition()
        {
            // Arrange
            var commandId = "cmd1";
            var robotId = "1";

            var command = new Command
            {
                Id = commandId,
                CommandType = "move",
                RobotId = robotId,
                Status = "Pending",
                Parameters = new Dictionary<string, object>
                {
                    { "direction", "forward" },
                    { "distance", 2.0 }
                }
            };

            var robot = new Robot
            {
                Id = robotId,
                Name = "Robot1",
                Status = "Idle",
                CurrentPosition = new Position
                {
                    X = 0,
                    Y = 0,
                    Z = 0,
                    Rotation = 0
                }
            };

            _mockCommandRepository.Setup(r => r.GetAsync(commandId)).ReturnsAsync(command);

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync(robot);

            _mockCommandRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Command>()))
                .ReturnsAsync((Command c) => c);

            _mockRobotRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Robot>()))
                .ReturnsAsync((Robot r) => r);

            // Act
            var result = await _commandService.ExecuteCommandAsync(commandId);

            // Assert
            Assert.True(result);

            _mockRobotRepository.Verify(
                r =>
                    r.UpdateAsync(
                        It.Is<Robot>(
                            robot =>
                                robot.Status == "Moving"
                                && robot.CurrentPosition.X == 2.0
                                && robot.CurrentPosition.Y == 0.0
                        )
                    ),
                Times.Once
            );

            _mockCommandRepository.Verify(
                r =>
                    r.UpdateAsync(
                        It.Is<Command>(cmd => cmd.Status == "Completed" && cmd.ExecutedAt.HasValue)
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ExecuteCommandAsync_CommandNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var commandId = "nonexistent";

            _mockCommandRepository.Setup(r => r.GetAsync(commandId)).ReturnsAsync((Command)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _commandService.ExecuteCommandAsync(commandId)
            );

            _mockCommandRepository.Verify(r => r.UpdateAsync(It.IsAny<Command>()), Times.Never);
            _mockRobotRepository.Verify(r => r.UpdateAsync(It.IsAny<Robot>()), Times.Never);
        }
    }
}
