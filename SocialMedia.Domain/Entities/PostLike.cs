namespace SocialMedia.Domain.Entities;

public class PostLike
{
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    
    public required string UserId { get; set; }
    public AppUser? User { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}