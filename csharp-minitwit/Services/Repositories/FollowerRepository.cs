using System.Diagnostics;

using csharp_minitwit.Services.Interfaces;
using csharp_minitwit.Utils;

using Microsoft.EntityFrameworkCore;

namespace csharp_minitwit.Services.Repositories
{
    public class FollowerRepository(MinitwitContext dbContext) : IFollowerRepository
    {
        public async Task<bool> Follow(int whoId, int whomId)
        {
            var watch = Stopwatch.StartNew();

            await dbContext.Followers.AddAsync(new Follower
            {
                WhoId = whoId,
                WhomId = whomId
            });

            watch.Stop();
            ApplicationMetrics.HttpRequestDuration
                    .WithLabels(MetricsHelpers.SanitizePath("/follow"))
                    .Observe(watch.Elapsed.TotalSeconds);

            return await dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> Unfollow(int whoId, int whomId)
        {
            var watch = Stopwatch.StartNew();
            var follower = await dbContext.Followers
                .FirstOrDefaultAsync(f => f.WhoId == whoId && f.WhomId == whomId);

            if (follower != null)
            {
                dbContext.Followers.Remove(follower);
                await dbContext.SaveChangesAsync();

                watch.Stop();
                ApplicationMetrics.HttpRequestDuration
                        .WithLabels(MetricsHelpers.SanitizePath("unfollow"))
                        .Observe(watch.Elapsed.TotalSeconds);

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> IsFollowing(int whoId, int whomId)
        {
            return await dbContext.Followers
                .AnyAsync(f => f.WhoId == whoId && f.WhomId == whomId);
        }

        public async Task<List<string>> GetFollowingNames(int n, int whoId)
        {
            return await dbContext.Followers
                .Where(f => f.WhoId == whoId)
                .Take(n)
                .Select(f => f.Whom.Username)
                .ToListAsync();
        }
    }
}
