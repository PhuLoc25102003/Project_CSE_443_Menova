using Menova.Models;
using Microsoft.EntityFrameworkCore;

namespace Menova.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> CheckUserExistsAsync(string username, string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username || u.Email == email);
        }

        public async Task UpdateUserProfileAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
            {
                throw new Exception("User not found");
            }

            existingUser.FullName = user.FullName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }
    }

}
