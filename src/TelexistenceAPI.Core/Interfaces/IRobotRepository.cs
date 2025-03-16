using TelexistenceAPI.Core.Entities;

namespace TelexistenceAPI.Core.Interfaces
{
    public interface IRobotRepository
    {
        Task<Robot?> GetAsync(string id);
        Task<IEnumerable<Robot>> GetAllAsync();
        Task<Robot> UpdateAsync(Robot robot);
    }
}
