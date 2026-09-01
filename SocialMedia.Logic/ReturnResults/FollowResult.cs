namespace SocialMedia.Logic.ReturnResults;

public abstract record FollowResult
{
    public sealed record CannotFollowSelf(string Message) : FollowResult;
    public sealed record Success : FollowResult;
    public sealed record TargetUserNotFound(string Message) : FollowResult;
    public sealed record AlreadyFollowing(string Message) : FollowResult;
}