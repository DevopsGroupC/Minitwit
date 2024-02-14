namespace csharp_minitwit.Services;

public interface IDatabaseService
{
    Task<IEnumerable<dynamic>> QueryDb(string sqlQuery, Dictionary<string, object> parameters);
}