using csharp_minitwit.Services;
using Microsoft.AspNetCore.Identity;

namespace csharp_minitwit.Utils
{
    public static class UserHelper
    {
        public static async Task<bool> IsUsernameTaken(IDatabaseService _databaseService, string username)
        {
            var sqlQuery = "SELECT * FROM user WHERE username = @Username";
            var parameters = new Dictionary<string, object> { { "@Username", username } };
            var result = await _databaseService.QueryDb<dynamic>(sqlQuery, parameters);
            
            return result.Count() > 0;
        }
        
        public static async Task<dynamic> InsertUser(PasswordHasher<UserModel> _passwordHasher, IDatabaseService _databaseService, string username, string email, string password)
        {
            var sqlQuery = @"
                INSERT INTO user (username, email, pw_hash)
                VALUES (@Username, @Email, @Password)";

            var hashedPassword = _passwordHasher.HashPassword(new UserModel(), password);

            var parameters = new Dictionary<string, object>
            {
                { "@Username", username },
                { "@Email", email },
                { "@Password", hashedPassword }
            };

            return await _databaseService.QueryDb<dynamic>(sqlQuery, parameters);
        }
    }
}