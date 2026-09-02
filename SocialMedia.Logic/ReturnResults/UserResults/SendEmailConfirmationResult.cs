namespace SocialMedia.Logic.ReturnResults;

public abstract record SendEmailConfirmationResult
{
    public sealed record Success(string Message) : SendEmailConfirmationResult;
    public sealed record EmailOrConfirmationLinkFailed(string Message) : SendEmailConfirmationResult;
    public sealed record SendingEmailFailed(string Message) : SendEmailConfirmationResult;
}