namespace csharp_minitwit.Services;

public interface IDatabaseService
{
    Task<dynamic> QueryDb(string SqlQuery);
}