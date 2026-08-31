namespace SocialMedia.Logic.ReturnResults;

public abstract record ConfirmEmailResult
{
    public sealed record InvalidEmailRequest(string Message) : ConfirmEmailResult;
    public sealed record InvalidTokenFormat(string Message) : ConfirmEmailResult;
    public sealed record InvalidTokenOrExpired(string Message) : ConfirmEmailResult;
    public sealed record UserNotFound(string Message) : ConfirmEmailResult;
    public sealed record Success(string Message) : ConfirmEmailResult;
}