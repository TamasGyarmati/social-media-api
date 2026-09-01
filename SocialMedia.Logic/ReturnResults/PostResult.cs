using SocialMedia.Domain.Enums;

namespace SocialMedia.Logic.ReturnResults;

public record PostResult(PostStatus Status, Guid? PostId = null)
{
    public static PostResult Success(Guid? id) => new(PostStatus.Success, id);
    public static PostResult NotFound() => new(PostStatus.NotFound);
    public static PostResult Forbidden() => new(PostStatus.Forbidden);
    public static PostResult FailedToDeleteImage() => new(PostStatus.Failed);
}