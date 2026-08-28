namespace SocialMedia.Domain.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? ImageUrl { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    
    public AppUser? Creator { get; set; }
    public required string CreatedById { get; set; }
}