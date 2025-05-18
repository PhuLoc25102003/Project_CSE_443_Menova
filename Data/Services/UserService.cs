using Menova.Models;
using System.Security.Cryptography;
using System.Text;

namespace Menova.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = await _unitOfWork.Users.GetUserByUsernameAsync(username);

            // Check if user exists
            if (user == null)
                return null;

            // Check if password is correct (this is a simplified example)
            if (!VerifyPasswordHash(password, user.passwordHash, user.salt))
                return null;

            // Authentication successful
            return user;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetUserByUsernameAsync(username);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetUserByEmailAsync(email);
        }

        public async Task<bool> RegisterUserAsync(User user, string password)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("Password is required");

            if (await _unitOfWork.Users.CheckUserExistsAsync(user.Username, user.Email))
                throw new Exception("Username or Email is already taken");

            // Create password hash
            CreatePasswordHash(password, out string passwordHash, out string salt);

            user.passwordHash = passwordHash;
            user.salt = salt;
            user.CreateAt = DateTime.Now;
            user.UpdateAt = DateTime.Now;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task UpdateUserProfileAsync(User user)
        {
            await _unitOfWork.Users.UpdateUserProfileAsync(user);
        }

        // Helper methods for password hashing
        private static void CreatePasswordHash(string password, out string passwordHash, out string salt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));

            using (var hmac = new HMACSHA512())
            {
                salt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));

            var saltBytes = Convert.FromBase64String(storedSalt);
            var storedHashBytes = Convert.FromBase64String(storedHash);

            using (var hmac = new HMACSHA512(saltBytes))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHashBytes[i]) return false;
                }
            }

            return true;
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users.Count();
        }
    }

}
