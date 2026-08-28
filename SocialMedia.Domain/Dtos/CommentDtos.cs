using SocialMedia.Domain.Entities;

namespace SocialMedia.Domain.Dtos;

public record CommentResponseShorterDto(
    Guid Id,
    string Content,
    DateTime CreatedAtUnc,
    DateTime? UpdatedAtUtc,
    DateTime? DeletedAtUtc,
    Guid? ParentCommentId,
    string CreatedById,
    int Likes,
    int ReplyCount
);
    
public record CommentResponseDto(
    Guid Id,
    string Content,
    DateTime CreatedAtUnc,
    DateTime? UpdatedAtUtc,
    DateTime? DeletedAtUtc,
    Guid PostId,
    Guid? ParentCommentId,
    string CreatedById,
    int Likes,
    int ReplyCount
);

public record CommentShortDto(
    string Content,
    DateTime CreatedAtUnc,
    string CreatedById,
    int ReplyCount
);

public record UpdateCommentDto(
    string Content
);

public record CreateCommentDto(
    string Content,
    Guid PostId,
    Guid? ParentCommentId = null
);

public static class CommentMappingExtensions
{
    public static CommentResponseShorterDto FromDomainToCommentResponseShorterDto(this Comment comment)
    {
        return new CommentResponseShorterDto(
            comment.Id,
            comment.Content,
            comment.CreatedAtUtc,
            comment.UpdatedAtUtc,
            comment.DeletedAtUtc,
            comment.ParentCommentId,
            comment.CreatedById,
            comment.Likes.Count,
            comment.Replies.Count
        );
    }
    
    public static CommentResponseDto FromDomainToCommentResponseDto(this Comment comment)
    {
        return new CommentResponseDto(
            comment.Id,
            comment.Content,
            comment.CreatedAtUtc,
            comment.UpdatedAtUtc,
            comment.DeletedAtUtc,
            comment.PostId,
            comment.ParentCommentId,
            comment.CreatedById,
            comment.Likes.Count,
            comment.Replies.Count
        );
    }
    
    public static void UpdateFromDto(this Comment comment, UpdateCommentDto dto)
    {
        comment.Content = dto.Content;
        comment.UpdatedAtUtc = DateTime.UtcNow;
    }
    
    public static Comment FromCreateCommentToDomain(this CreateCommentDto dto, string creatorId)
    {
        return new Comment
        {
            Content = dto.Content,
            PostId = dto.PostId,
            CreatedById = creatorId,
            ParentCommentId = dto.ParentCommentId
        };
    }
}