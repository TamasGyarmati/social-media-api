namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record SoftDeleteUserResult
{
    public sealed record DeletionFailed(string Message) : SoftDeleteUserResult;
    public sealed record Success(string Message) : SoftDeleteUserResult;
    public sealed record Forbidden(string Message) : SoftDeleteUserResult;
}