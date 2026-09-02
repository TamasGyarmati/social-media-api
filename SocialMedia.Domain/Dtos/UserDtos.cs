using Microsoft.AspNetCore.Http;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Domain.Dtos;

public record GetUserDto(
    string Id,
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string? AvatarUrl,
    int FollowerCount,
    int FollowingCount,
    List<string> FollowerUsernames,
    List<string> FollowingUsernames
);

public record UploadAvatarDto(
    IFormFile Image
);

public record UpdateUserDto(
    string? FirstName,
    string? LastName,
    string? UserName,
    IFormFile? Avatar
);

public record UpdatePasswordDto(
    string OldPassword,
    string NewPassword
);

public record RequestEmailChangeDto(
    string NewEmail
);

public static class UserMappingExtensions
{
    public static GetUserDto FromDomainToGetUserDto(this AppUser user)
    {
        return new GetUserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.AvatarUrl,
            user.Followers.Count,
            user.Following.Count,
            user.Followers
                .Select(x => x.Follower.UserName ?? string.Empty)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToList(),
            user.Following
                .Select(x => x.Followed.UserName ?? string.Empty)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .ToList()
        );
    }
}
