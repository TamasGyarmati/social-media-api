using SocialMedia.Domain.Dtos;

namespace SocialMedia.Logic.Helpers;

public abstract record LoginResult
{
    public sealed record EmailUnconfirmed(string Message) : LoginResult;
    public sealed record UserOrPasswordNotExist(string Message) : LoginResult;
    public sealed record Success(LoginResultDto TokenWithExpiryDate) : LoginResult;
}