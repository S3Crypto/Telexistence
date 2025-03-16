using TelexistenceAPI.Core.Entities;

namespace TelexistenceAPI.Core.Interfaces
{
    public interface ICommandRepository
    {
        Task<Command> CreateAsync(Command command);
        Task<Command?> GetAsync(string id);
        Task<Command> UpdateAsync(Command command);
        Task<IEnumerable<Command>> GetHistoryAsync(string robotId, int limit = 10);
    }
}
