namespace SocialMedia.Domain.Entities;

public class UserFollow
{
    public string FollowerId { get; set; } = null!;
    public AppUser Follower { get; set; } = null!;
    
    public string FollowedId { get; set; } = null!;
    public AppUser Followed { get; set; } = null!;
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}