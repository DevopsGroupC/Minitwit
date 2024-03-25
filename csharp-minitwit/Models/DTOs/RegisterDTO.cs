using System.ComponentModel.DataAnnotations;

namespace csharp_minitwit.Models.DTOs;

public class RegisterDTO
{
    [Required(ErrorMessage = "You have to enter a username")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "You have to enter a valid email address")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "You have to enter a password")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "You must confirm your password")]
    [Compare("Password", ErrorMessage = "The two passwords do not match")]
    public required string Password2 { get; set; }
}