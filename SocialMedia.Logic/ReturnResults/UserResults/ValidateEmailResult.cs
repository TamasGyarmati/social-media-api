using SocialMedia.Domain.Entities;

namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record ValidateEmailResult
{
    public sealed record Success(AppUser User) : ValidateEmailResult;
    
    public abstract record Failure(string Message) : ValidateEmailResult;
    public sealed record NewEmailFailed(string Message) : Failure(Message);
    public sealed record UserNotFound(string Message) : Failure(Message);
    public sealed record UserWithEmailExist(string Message) : Failure(Message);
}