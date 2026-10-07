namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record LogoutUserResult
{
    public sealed record Success(string Message) : LogoutUserResult;
    public sealed record FailedToLogout(string Message) : LogoutUserResult;
}