using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using csharp_minitwit.Models;
using Dapper;

namespace csharp_minitwit.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<T>> QueryDb<T>(string sqlQuery, Dictionary<string, object> parameters)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QueryAsync<T>(sqlQuery, parameters);
        }
    }
}