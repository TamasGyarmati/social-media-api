namespace SocialMedia.Logic.ReturnResults;

public abstract record PostResult
{
    public sealed record Success(Guid? Id) : PostResult;
    public sealed record NotFound(string Message) : PostResult;
    public sealed record FailedToDeleteImage(string Message) : PostResult;
    public sealed record Forbidden(string Message) : PostResult;
}