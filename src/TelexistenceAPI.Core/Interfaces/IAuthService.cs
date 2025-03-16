namespace TelexistenceAPI.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(string token, DateTime expiration)> AuthenticateAsync(
            string username,
            string password
        );
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
