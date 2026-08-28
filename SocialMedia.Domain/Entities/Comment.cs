namespace SocialMedia.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public required string Content { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    
    public Guid PostId { get; set; }
    public Post? Post { get; set; }
    
    public Guid? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    
    public AppUser? Creator { get; set; }
    public required string CreatedById { get; set; }
}