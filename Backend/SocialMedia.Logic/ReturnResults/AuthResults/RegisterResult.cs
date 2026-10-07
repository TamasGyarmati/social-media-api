using Microsoft.AspNetCore.Identity;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Logic.ReturnResults.AuthResults;

public abstract record RegisterResult
{
    public sealed record Success(AppUser User, string EncodedToken) : RegisterResult;
    public sealed record Failed(IEnumerable<IdentityError> Errors) : RegisterResult;
}