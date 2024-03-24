using System.Diagnostics;

using csharp_minitwit.Models;
using csharp_minitwit.Models.ViewModels;
using csharp_minitwit.Services.Interfaces;
using csharp_minitwit.Utils;

using Microsoft.EntityFrameworkCore;

namespace csharp_minitwit.Services.Repositories
{
    public class MessageRepository(MinitwitContext dbContext) : IMessageRepository
    {
        public Task AddMessageAsync(string text, int authorId)
        {
            var watch = Stopwatch.StartNew();
            var message = new Message
            {
                Text = text,
                AuthorId = authorId,
                PubDate = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
            };

            dbContext.Messages.Add(message);

            watch.Stop();
            ApplicationMetrics.HttpRequestDuration
                    .WithLabels(MetricsHelpers.SanitizePath("add_message"))
                    .Observe(watch.Elapsed.TotalSeconds);

            return dbContext.SaveChangesAsync();
        }

        public async Task<List<MessageWithAuthorModel>> GetMessagesWithAuthorAsync(int n)
        {
            return await dbContext.Messages
                .Where(m => m.Flagged == 0)
                .Join(dbContext.Users,
                    message => message.AuthorId,
                    user => user.UserId,
                    (message, user) => new MessageWithAuthorModel
                    {
                        Message = message,
                        Author = user,
                    })
                .OrderByDescending(ma => ma.Message.PubDate)
                .Take(n)
                .ToListAsync();
        }

        public async Task<List<MessageWithAuthorModel>> GetMessagesByAuthorAsync(int n, int authorId)
        {
            return await dbContext.Messages
                .Where(m => m.Flagged == 0 && m.AuthorId == authorId)
                .Join(dbContext.Users,
                    message => message.AuthorId,
                    user => user.UserId,
                    (message, user) => new MessageWithAuthorModel
                    {
                        Message = message,
                        Author = user,
                    })
                .OrderByDescending(ma => ma.Message.PubDate)
                .Take(n)
                .ToListAsync();
        }

        public async Task<List<MessageWithAuthorModel>> GetFollowedMessages(int n, int userId)
        {
            return await dbContext.Messages
                .Where(m => m.Flagged == 0)
                .Join(dbContext.Users,
                    message => message.AuthorId,
                    user => user.UserId,
                    (message, user) => new MessageWithAuthorModel
                    {
                        Message = message,
                        Author = user,
                    })
                .Where(ma =>
                    ma.Author.UserId == userId
                    || dbContext.Followers.Any(f => f.WhoId == userId && f.WhomId == ma.Author.UserId))
                .OrderByDescending(ma => ma.Message.PubDate)
                .Take(n)
                .ToListAsync();
        }

        public async Task<List<APIMessageModel>> GetApiMessagesAsync(int n)
        {
            return await dbContext.Messages
                .Where(m => m.Flagged == 0)
                .OrderByDescending(m => m.PubDate)
                .Take(n)
                .Select(m => new APIMessageModel
                {
                    content = m.Text,
                    pub_date = m.PubDate,
                    user = m.Author.Username
                })
                .ToListAsync();
        }

        public async Task<List<APIMessageModel>> GetApiMessagesByAuthorAsync(int n, int authorId)
        {
            return await dbContext.Messages
                .Where(m => m.AuthorId == authorId && m.Flagged == 0)
                .OrderByDescending(m => m.PubDate)
                .Take(n)
                .Select(m => new APIMessageModel
                {
                    content = m.Text,
                    pub_date = m.PubDate,
                    user = m.Author.Username
                })
                .ToListAsync();
        }
    }
}