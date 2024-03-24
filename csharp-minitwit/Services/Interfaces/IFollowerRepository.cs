namespace csharp_minitwit.Services.Interfaces;

public interface IFollowerRepository
{
    Task<bool> Follow(int whoId, int whomId);
    Task<bool> Unfollow(int whoId, int whomId);
    Task<bool> IsFollowing(int whoId, int whomId);
    Task<List<string>> GetFollowingNames(int n, int whoId);
}