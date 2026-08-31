using Microsoft.AspNetCore.Identity;

namespace SocialMedia.Domain.Entities;

public class AppUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string RefreshToken { get; set; }
    public string? AvatarUrl { get; set; }
}