namespace csharp_minitwit.Services;

public interface IDatabaseService
{
    Task<IEnumerable<dynamic>> QueryDb(string sqlQuery);
}