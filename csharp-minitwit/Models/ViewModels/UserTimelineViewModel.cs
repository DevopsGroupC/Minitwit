using csharp_minitwit.Models.Entities;

namespace csharp_minitwit.Models.ViewModels
{
    public class UserTimelineViewModel
    {
        public int? CurrentUserId { get; set; }
        public User? ProfileUser { get; set; }
        public List<MessageWithAuthorModel>? MessagesWithAuthor { get; set; }
        public bool Followed { get; set; }
    }

}