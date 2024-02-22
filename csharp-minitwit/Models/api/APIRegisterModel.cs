namespace csharp_minitwit.Models
{
    public class APIRegisterModel
    {
        public required string? username { get; set; }
        public required string? email { get; set; }
        public required string? pwd { get; set; }
    }
}
