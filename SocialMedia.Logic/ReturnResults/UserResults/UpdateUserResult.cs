namespace SocialMedia.Logic.ReturnResults;

public abstract record UpdateUserResult
{
    public sealed record UserNotFound(string Message) : UpdateUserResult;
    public sealed record UserNameChangeFailed(string Message) : UpdateUserResult;
    public sealed record AvatarDeletionFailed(string Message) : UpdateUserResult;
    public sealed record FailedToUpdateUser(string Message) : UpdateUserResult;
    public sealed record Success(string Message) : UpdateUserResult;
}