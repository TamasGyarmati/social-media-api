using Microsoft.AspNetCore.Identity;

namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record AddToAdminResult
{
    public sealed record Success(string Message) : AddToAdminResult;
    public sealed record UserNotFound(string Message) : AddToAdminResult;
    public sealed record RoleAdditionFailed(string Message, IEnumerable<IdentityError> Errors) : AddToAdminResult;
}