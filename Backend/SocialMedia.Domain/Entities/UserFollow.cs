namespace SocialMedia.Domain.Entities;

public class UserFollow
{
    public required string FollowerId { get; set; }
    public AppUser? Follower { get; set; }
    
    public required string FollowedId { get; set; }
    public AppUser? Followed { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}