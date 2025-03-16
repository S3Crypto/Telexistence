using MongoDB.Driver;
using TelexistenceAPI.Core.Entities;
using TelexistenceAPI.Core.Interfaces;

namespace TelexistenceAPI.Repositories
{
    public class CommandRepository : ICommandRepository
    {
        private readonly IMongoCollection<Command> _commands;

        public CommandRepository(MongoDbContext context)
        {
            _commands = context.GetCollection<Command>("Commands");
        }

        public async Task<Command> CreateAsync(Command command)
        {
            await _commands.InsertOneAsync(command);
            return command;
        }

        public async Task<Command?> GetAsync(string id)
        {
            return await _commands.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Command> UpdateAsync(Command command)
        {
            await _commands.ReplaceOneAsync(c => c.Id == command.Id, command);
            return command;
        }

        public async Task<IEnumerable<Command>> GetHistoryAsync(string robotId, int limit = 10)
        {
            return await _commands
                .Find(c => c.RobotId == robotId)
                .SortByDescending(c => c.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }
    }
}
