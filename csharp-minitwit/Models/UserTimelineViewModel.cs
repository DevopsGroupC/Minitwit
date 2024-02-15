namespace csharp_minitwit.Models
{
    public class UserTimelineViewModel
    {
        public int? currentUserId { get; set; }
        public UserModel? profileUser { get; set; }
        public List<MessageModel> messages { get; set; }
        public bool followed { get; set; }
    }

}