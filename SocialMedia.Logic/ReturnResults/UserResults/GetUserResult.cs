using SocialMedia.Domain.Dtos;

namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record GetUserResult
{
    public sealed record UserNotFound(string Message) : GetUserResult;
    public sealed record Success(GetUserDto User) : GetUserResult;
}