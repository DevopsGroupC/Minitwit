using csharp_minitwit.Models.Entities;

namespace csharp_minitwit.Models.ViewModels
{
    public class MessageWithAuthorModel
    {
        public required Message Message { get; set; }
        public required User Author { get; set; }
    }
}