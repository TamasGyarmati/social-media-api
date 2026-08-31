using SocialMedia.Domain.Enums;

namespace SocialMedia.Logic.ReturnResults;

public record CommentResult(CommentStatus Status, Guid? CommentId = null)
{
    public static CommentResult Success(Guid? id) => new(CommentStatus.Success, id);
    public static CommentResult NotFound() => new(CommentStatus.NotFound);
    public static CommentResult Forbidden() => new(CommentStatus.Forbidden);
}