using Microsoft.AspNetCore.Identity;

namespace SocialMedia.Domain.Entities;

public class AppUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? AvatarUrl { get; set; }
    
    public ICollection<UserFollow> Followers { get; set; } = new List<UserFollow>(); 
    public ICollection<UserFollow> Following { get; set; } = new List<UserFollow>();
}