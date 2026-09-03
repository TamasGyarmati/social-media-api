using SocialMedia.Domain.Entities;

namespace SocialMedia.Domain.Dtos;

public record CommentResponseDto(
    Guid Id,
    string Content,
    DateTime CreatedAtUnc,
    DateTime? UpdatedAtUtc,
    DateTime? DeletedAtUtc,
    Guid PostId,
    Guid? ParentCommentId,
    string CreatedById,
    string UserName,
    int Likes,
    int ReplyCount
);

public record CommentShortDto(
    string Content,
    DateTime CreatedAtUnc,
    string CreatedById,
    int ReplyCount
);

public record UpdateCommentRequestDto(
    string Content
);

public record CreateCommentRequestDto(
    string Content,
    Guid PostId,
    Guid? ParentCommentId = null
);

public record CreateCommentResponseDto(
    Guid Id
);

public record UpdateCommentResponseDto(
    Guid? Id
);

public record CreateCommentLikeResponseDto(
    bool IsLiked, 
    string Message
);

public static class CommentMappingExtensions
{
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
            comment.Creator?.UserName ?? string.Empty,
            comment.Likes.Count,
            comment.Replies.Count
        );
    }
    
    public static void UpdateFromDto(this Comment comment, UpdateCommentRequestDto dto)
    {
        comment.Content = dto.Content;
        comment.UpdatedAtUtc = DateTime.UtcNow;
    }
    
    public static Comment FromCreateCommentToDomain(this CreateCommentRequestDto dto, string creatorId)
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