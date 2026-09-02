namespace SocialMedia.Logic.ReturnResults;

public abstract record ConfirmEmailChangeResult
{
    public sealed record ArgumentsRequired(string Message) : ConfirmEmailChangeResult;
    public sealed record UserNotFound(string Message) : ConfirmEmailChangeResult;
    public sealed record EmailChangeFailed(string Message) : ConfirmEmailChangeResult;
    public sealed record Success(string Message) : ConfirmEmailChangeResult;
}