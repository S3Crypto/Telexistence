using Microsoft.Extensions.Logging;
using Moq;
using TelexistenceAPI.Core.Entities;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.Core.Services;
using Xunit;

namespace TelexistenceAPI.Tests.Services
{
    public class RobotServiceTests
    {
        private readonly Mock<IRobotRepository> _mockRobotRepository;
        private readonly Mock<ILogger<RobotService>> _mockLogger;
        private readonly RobotService _robotService;

        public RobotServiceTests()
        {
            _mockRobotRepository = new Mock<IRobotRepository>();
            _mockLogger = new Mock<ILogger<RobotService>>();
            _robotService = new RobotService(_mockRobotRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetRobotAsync_ValidId_ReturnsRobot()
        {
            // Arrange
            var robotId = "1";
            var expectedRobot = new Robot
            {
                Id = robotId,
                Name = "TX-010",
                Status = "Idle"
            };

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync(expectedRobot);

            // Act
            var result = await _robotService.GetRobotAsync(robotId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRobot.Id, result.Id);
            Assert.Equal(expectedRobot.Name, result.Name);
            Assert.Equal(expectedRobot.Status, result.Status);
        }

        [Fact]
        public async Task GetRobotAsync_InvalidId_ReturnsNull()
        {
            // Arrange
            var robotId = "nonexistent";

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync((Robot)null);

            // Act
            var result = await _robotService.GetRobotAsync(robotId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllRobotsAsync_ReturnsAllRobots()
        {
            // Arrange
            var robots = new List<Robot>
            {
                new Robot
                {
                    Id = "1",
                    Name = "TX-010",
                    Status = "Idle"
                },
                new Robot
                {
                    Id = "2",
                    Name = "TX-020",
                    Status = "Moving"
                }
            };

            _mockRobotRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(robots);

            // Act
            var result = await _robotService.GetAllRobotsAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal("1", resultList[0].Id);
            Assert.Equal("2", resultList[1].Id);
        }

        [Fact]
        public async Task UpdateRobotStatusAsync_ValidId_UpdatesStatus()
        {
            // Arrange
            var robotId = "1";
            var newStatus = "Moving";
            var newTask = "Moving forward";

            var robot = new Robot
            {
                Id = robotId,
                Name = "TX-010",
                Status = "Idle",
                CurrentTask = null
            };

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync(robot);

            _mockRobotRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Robot>()))
                .ReturnsAsync((Robot r) => r);

            // Act
            var result = await _robotService.UpdateRobotStatusAsync(robotId, newStatus, newTask);

            // Assert
            Assert.Equal(newStatus, result.Status);
            Assert.Equal(newTask, result.CurrentTask);
            Assert.NotEqual(default, result.LastUpdated);

            _mockRobotRepository.Verify(
                r =>
                    r.UpdateAsync(
                        It.Is<Robot>(
                            robot =>
                                robot.Id == robotId
                                && robot.Status == newStatus
                                && robot.CurrentTask == newTask
                        )
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateRobotStatusAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var robotId = "nonexistent";
            var newStatus = "Moving";

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync((Robot)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _robotService.UpdateRobotStatusAsync(robotId, newStatus)
            );

            _mockRobotRepository.Verify(r => r.UpdateAsync(It.IsAny<Robot>()), Times.Never);
        }

        [Fact]
        public async Task UpdateRobotPositionAsync_ValidId_UpdatesPosition()
        {
            // Arrange
            var robotId = "1";
            var newPosition = new Position
            {
                X = 10,
                Y = 20,
                Z = 0,
                Rotation = 90
            };

            var robot = new Robot
            {
                Id = robotId,
                Name = "TX-010",
                Status = "Idle",
                CurrentPosition = new Position
                {
                    X = 0,
                    Y = 0,
                    Z = 0,
                    Rotation = 0
                }
            };

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync(robot);

            _mockRobotRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Robot>()))
                .ReturnsAsync((Robot r) => r);

            // Act
            var result = await _robotService.UpdateRobotPositionAsync(robotId, newPosition);

            // Assert
            Assert.Equal(newPosition.X, result.CurrentPosition.X);
            Assert.Equal(newPosition.Y, result.CurrentPosition.Y);
            Assert.Equal(newPosition.Z, result.CurrentPosition.Z);
            Assert.Equal(newPosition.Rotation, result.CurrentPosition.Rotation);
            Assert.NotEqual(default, result.LastUpdated);

            _mockRobotRepository.Verify(
                r =>
                    r.UpdateAsync(
                        It.Is<Robot>(
                            robot =>
                                robot.Id == robotId
                                && robot.CurrentPosition.X == newPosition.X
                                && robot.CurrentPosition.Y == newPosition.Y
                                && robot.CurrentPosition.Z == newPosition.Z
                                && robot.CurrentPosition.Rotation == newPosition.Rotation
                        )
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task UpdateRobotPositionAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var robotId = "nonexistent";
            var newPosition = new Position
            {
                X = 10,
                Y = 20,
                Z = 0,
                Rotation = 90
            };

            _mockRobotRepository.Setup(r => r.GetAsync(robotId)).ReturnsAsync((Robot)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _robotService.UpdateRobotPositionAsync(robotId, newPosition)
            );

            _mockRobotRepository.Verify(r => r.UpdateAsync(It.IsAny<Robot>()), Times.Never);
        }
    }
}
