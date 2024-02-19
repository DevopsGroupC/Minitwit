using Microsoft.Data.Sqlite;
using Dapper;

namespace csharp_minitwit.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var dbFilePath = new SqliteConnectionStringBuilder(_connectionString).DataSource;
        if (!File.Exists(dbFilePath) )
        {
            initDb(dbFilePath);
        }
    }

    private void initDb(string dbFilePath)
    {
        var directory = Path.GetDirectoryName(dbFilePath);
        var sqlFilePath = Path.Combine(directory!, "schema.sql"); //Todo: don't hardcode.
        
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var sqlCommands = File.ReadAllText(sqlFilePath);
            var command = connection.CreateCommand();

            command.CommandText = sqlCommands;
            command.ExecuteNonQuery();
        }
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