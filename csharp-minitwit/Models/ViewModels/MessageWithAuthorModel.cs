namespace csharp_minitwit.Models.ViewModels
{
    public class MessageWithAuthorModel
    {
        public Message Message { get; set; }
        public User Author { get; set; }
    }
}