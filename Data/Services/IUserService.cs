using Menova.Models;

namespace Menova.Data.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> RegisterUserAsync(User user, string password);
        Task UpdateUserProfileAsync(User user);

        Task<int> GetTotalUserCountAsync();
    }

}
