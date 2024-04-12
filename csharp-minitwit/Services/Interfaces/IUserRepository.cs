using csharp_minitwit.Models.Entities;

namespace csharp_minitwit.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsername(string username);
    Task<bool> UserExists(string username);
    Task<bool> InsertUser(string username, string email, string password);
    Task<int?> GetUserIdAsync(string username);
}