using csharp_minitwit.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace csharp_minitwit.Services.Repositories
{
    public class UserRepository(MinitwitContext dbContext) : IUserRepository
    {
        private static readonly PasswordHasher<User> PasswordHasher = new();

        public async Task<int?> GetUserIdAsync(string username)
        {
            return await dbContext.Users
                .Where(u => u.Username == username)
                .Select(u => (int?)u.UserId)
                .FirstOrDefaultAsync();
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await dbContext.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UserExists(string username)
        {
            return await dbContext.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> InsertUser(string username, string email, string password)
        {
            try
            {
                var user = new User
                {
                    Username = username,
                    Email = email,
                };

                user.PwHash = PasswordHasher.HashPassword(user, password);

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Todo: Add logging
                return false;
            }
        }
    }
}
