using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CheckUserExistsAsync(string username, string email);
        Task UpdateUserProfileAsync(User user);
    }
}
