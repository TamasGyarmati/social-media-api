using Microsoft.AspNetCore.Http;

namespace SocialMedia.Domain.Dtos;

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

public record ConfirmEmailChangeDto(
    string UserId,
    string NewEmail,
    string Token
);