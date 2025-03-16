using MongoDB.Driver;
using TelexistenceAPI.Core.Entities;
using TelexistenceAPI.Core.Interfaces;
using TelexistenceAPI.Repositories;

namespace TelexistenceAPI.Middleware
{
    public static class SeedData
    {
        public static async Task InitializeAsync(MongoDbContext context, IAuthService authService)
        {
            // Seed robots
            var robotCollection = context.GetCollection<Robot>("Robots");
            if (!await robotCollection.Find(_ => true).AnyAsync())
            {
                var robots = new List<Robot>
                {
                    new Robot
                    {
                        Id = "1",
                        Name = "TX-010",
                        CurrentPosition = new Position
                        {
                            X = 0,
                            Y = 0,
                            Z = 0,
                            Rotation = 0
                        },
                        Status = "Idle",
                        CurrentTask = null,
                        LastUpdated = DateTime.UtcNow
                    },
                    new Robot
                    {
                        Id = "2",
                        Name = "TX-020",
                        CurrentPosition = new Position
                        {
                            X = 5,
                            Y = 5,
                            Z = 0,
                            Rotation = 90
                        },
                        Status = "Idle",
                        CurrentTask = null,
                        LastUpdated = DateTime.UtcNow
                    }
                };

                await robotCollection.InsertManyAsync(robots);
            }

            // Seed users
            var userCollection = context.GetCollection<User>("Users");
            if (!await userCollection.Find(_ => true).AnyAsync())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Id = "1",
                        Username = "admin",
                        PasswordHash = authService.HashPassword("admin123"),
                        Roles = new List<string> { "Admin", "Operator" },
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = "2",
                        Username = "operator",
                        PasswordHash = authService.HashPassword("operator123"),
                        Roles = new List<string> { "Operator" },
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await userCollection.InsertManyAsync(users);
            }
        }
    }
}
