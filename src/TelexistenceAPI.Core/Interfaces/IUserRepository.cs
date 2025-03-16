using TelexistenceAPI.Core.Entities;

namespace TelexistenceAPI.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
    }
}
