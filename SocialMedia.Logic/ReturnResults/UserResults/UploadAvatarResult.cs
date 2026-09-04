namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record UploadAvatarResult
{
    public sealed record UserNotFound(string Message) : UploadAvatarResult;
    public sealed record ImageProcessingError(string Message) : UploadAvatarResult;
    public sealed record FailedToUpdateAvatar(string Message) : UploadAvatarResult;
    public sealed record Success(string RelativePath) : UploadAvatarResult;
}