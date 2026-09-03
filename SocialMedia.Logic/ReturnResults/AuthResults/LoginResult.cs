using SocialMedia.Domain.Dtos;

namespace SocialMedia.Logic.ReturnResults.AuthResults;

public abstract record LoginResult
{
    public sealed record EmailUnconfirmed(string Message) : LoginResult;
    public sealed record UserOrPasswordNotExist(string Message) : LoginResult;
    public sealed record Success(UserLoginResponseDto TokenWithExpiryDate) : LoginResult;
}