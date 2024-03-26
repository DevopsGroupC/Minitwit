namespace csharp_minitwit.Models.ViewModels
{
    public class RegisterViewModel
    {
        public required string? Username { get; set; }
        public required string? Email { get; set; }
        public required string? Password { get; set; }
        public required string? Password2 { get; set; }
    }
}