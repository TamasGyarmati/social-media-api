using Microsoft.AspNetCore.Http;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Domain.Dtos;

public record GetUserByIdResponseDto(
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

public record UploadAvatarRequestDto(
    IFormFile Image
);

public record UpdateUserRequestDto(
    string? FirstName,
    string? LastName,
    string? UserName,
    IFormFile? Avatar
);

public record UpdatePasswordRequestDto(
    string OldPassword,
    string NewPassword
);

public record EmailChangeRequestDto(
    string NewEmail
);

public record UploadAvatarResponseDto(
    string AvatarUrl
);

public record UserMessageResponseDto(
    string Message
);

public record FollowUserResponseDto(
    string Message,
    string FollowedId
);

public static class UserMappingExtensions
{
    public static GetUserByIdResponseDto FromDomainToGetUserDto(this AppUser user)
    {
        return new GetUserByIdResponseDto(
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
