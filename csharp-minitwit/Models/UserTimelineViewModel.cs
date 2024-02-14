public class UserTimelineViewModel
{
    public UserModel profileUser { get; set; }
    public List<MessageModel> messages { get; set; }
    public bool followed { get; set; }
}