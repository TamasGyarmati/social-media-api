using SocialMedia.Domain.Dtos;

namespace SocialMedia.Logic.ReturnResults.UserResults;

public abstract record GetUserByIdResult
{
    public sealed record UserNotFound(string Message) : GetUserByIdResult;
    public sealed record Success(GetUserByIdResponseDto User) : GetUserByIdResult;
}