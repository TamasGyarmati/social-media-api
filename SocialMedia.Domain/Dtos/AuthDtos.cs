namespace SocialMedia.Domain.Dtos;

public record UserCreateDto(
    string FirstName,
    string LastName,
    string UserName,
    string Email, 
    string Password
);

public record UserLoginDto(
    string Email,
    string Password
);

public record LoginResultDto(
    string AccessToken, 
    DateTime AccessTokenExpireDate,
    string RefreshToken,
    DateTime RefreshTokenExpireDate
);

public record TokenApiDto(
    string AccessToken,
    string RefreshToken
);