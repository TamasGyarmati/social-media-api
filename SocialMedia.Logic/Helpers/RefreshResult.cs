using SocialMedia.Domain.Dtos;

namespace SocialMedia.Logic.Helpers;

public abstract record RefreshResult
{
    public sealed record Invalid(string Message) : RefreshResult;
    public sealed record Success(AuthResponseDto Response) : RefreshResult;
}