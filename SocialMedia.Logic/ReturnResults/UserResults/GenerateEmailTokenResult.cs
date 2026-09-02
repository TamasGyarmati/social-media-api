namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record GenerateEmailTokenResult
{
    public sealed record EmailValidationFailed(string Message) : GenerateEmailTokenResult;
    public sealed record Success(string Token) : GenerateEmailTokenResult;
}