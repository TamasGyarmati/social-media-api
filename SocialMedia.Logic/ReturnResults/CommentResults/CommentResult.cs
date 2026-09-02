namespace SocialMedia.Logic.ReturnResults.CommentResults;

public abstract record CommentResult
{
    public sealed record Success(Guid? Id) : CommentResult;
    public sealed record NotFound(string Message) : CommentResult;
    public sealed record Forbidden(string Message) : CommentResult;
}