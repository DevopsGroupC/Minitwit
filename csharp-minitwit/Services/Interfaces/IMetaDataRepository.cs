namespace csharp_minitwit.Services.Interfaces;

public interface IMetaDataRepository
{
    Task<int> GetLatestAsync();
    Task<bool> SetLatestAsync(int latest);
}