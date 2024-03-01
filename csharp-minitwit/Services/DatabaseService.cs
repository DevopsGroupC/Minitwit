using Microsoft.Data.Sqlite;
using Dapper;

namespace csharp_minitwit.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var dbFilePath = new SqliteConnectionStringBuilder(_connectionString).DataSource;
        if (!File.Exists(dbFilePath) )
        {
            initDb(dbFilePath);
        }
    }

private void initDb(string dbFilePath)
{
    try
    {
        var dbDirectory = Path.GetDirectoryName(dbFilePath);
        var parentDirectory = Directory.GetParent(dbDirectory!)!.FullName;
        var sqlFilePath = Path.Combine(parentDirectory, "schema.sql"); // TODO: don't hardcode schema.sql

        // TODO: remove logging statements
        _logger.LogInformation($"Creating database at {dbFilePath}");
        _logger.LogInformation($"Getting schema from {sqlFilePath}");
        if (Directory.Exists(dbDirectory))
        {
            foreach (var file in Directory.GetFiles(dbDirectory))
            {
                _logger.LogInformation($"Found file in database directory: {file}");
            }
        }
        var sqlFileDirectory = Path.GetDirectoryName(sqlFilePath);
        if (sqlFileDirectory != null && Directory.Exists(sqlFileDirectory))
        {
            foreach (var file in Directory.GetFiles(sqlFileDirectory))
            {
                _logger.LogInformation($"Found file in SQL schema directory: {file}");
            }
        }

        if (!Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory!);
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
    catch (Exception ex)
    {
        _logger.LogError($"Error creating the database at {dbFilePath}: {ex.Message}");
        throw;
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