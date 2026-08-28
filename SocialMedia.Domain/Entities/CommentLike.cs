namespace SocialMedia.Domain.Entities;

public class CommentLike
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid CommentId { get; set; }
    public Comment? Comment { get; set; }
    
    public required string UserId { get; set; }
    public AppUser? User { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}