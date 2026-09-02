namespace SocialMedia.Logic.ReturnResults;

public abstract record UpdatePasswordResult
{
    public sealed record PasswordIsNullOrWhitespace(string Message) : UpdatePasswordResult;
    public sealed record UserNotFound(string Message) : UpdatePasswordResult;
    public sealed record PasswordChangeFailed(string Message) : UpdatePasswordResult;
    public sealed record Success(string Message) : UpdatePasswordResult;
}