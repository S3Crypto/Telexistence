using TelexistenceAPI.Core.Entities;

namespace TelexistenceAPI.Core.Interfaces
{
    public interface ICommandService
    {
        Task<Command> CreateCommandAsync(
            string commandType,
            string robotId,
            string userId,
            Dictionary<string, object>? parameters = null
        );
        Task<Command?> GetCommandAsync(string id);
        Task<Command> UpdateCommandAsync(
            string id,
            string commandType,
            Dictionary<string, object>? parameters = null
        );
        Task<IEnumerable<Command>> GetCommandHistoryAsync(string robotId, int limit = 10);
        Task<bool> ExecuteCommandAsync(string id);
    }
}
