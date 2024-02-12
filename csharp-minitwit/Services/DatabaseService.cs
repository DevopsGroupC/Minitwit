using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using csharp_minitwit.Models;
using Dapper;

namespace csharp_minitwit.Services;

public class DatabaseService(IConfiguration configuration) : IDatabaseService
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    private readonly int _perPage = configuration.GetValue<int>("Constants:PerPage")!;

    public async Task<dynamic> QueryDb(string sqlQuery)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();

            return await connection.QueryAsync<dynamic>(sqlQuery, new { PerPage = _perPage });
        }
    }
}