namespace csharp_minitwit.Services;

public interface IDatabaseService
{
    Task<IEnumerable<T>> QueryDb<T>(string sqlQuery, Dictionary<string, object> parameters);
}