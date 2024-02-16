namespace csharp_minitwit.Models
{
    public class UserTimelineModel
    {
        public int? CurrentUserId { get; set; }
        public UserModel? ProfileUser { get; set; }
        public List<MessageModel> Messages { get; set; }
        public bool Followed { get; set; }
    }

}