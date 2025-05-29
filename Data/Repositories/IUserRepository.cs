using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser> GetUserByUsernameAsync(string username);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<bool> CheckUserExistsAsync(string username, string email);
        Task UpdateUserProfileAsync(ApplicationUser user);
    }
}
