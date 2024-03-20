using csharp_minitwit.Models;
using csharp_minitwit.Models.ViewModels;

namespace csharp_minitwit.Services.Interfaces;

public interface IMessageRepository
{
    Task AddMessageAsync(string text, int authorId);
    Task<List<MessageWithAuthorModel>> GetMessagesWithAuthorAsync(int n);
    Task<List<MessageWithAuthorModel>> GetMessagesByAuthorAsync(int n, int authorId);
    Task<List<MessageWithAuthorModel>> GetFollowedMessages(int n, int userId);

    Task<List<APIMessageModel>> GetApiMessagesAsync(int n);
    Task<List<APIMessageModel>> GetApiMessagesByAuthorAsync(int n, int authorId);
}