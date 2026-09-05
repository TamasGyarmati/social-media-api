namespace SocialMedia.Domain.Dtos;

public record UserCreateRequestDto(
    string FirstName,
    string LastName,
    string UserName,
    string Email, 
    string Password
);

public record UserLoginRequestDto(
    string Email,
    string Password
);

public record RefreshRequestDto(
    string AccessToken,
    string RefreshToken
);

public record RefreshResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiration,
    DateTime RefreshTokenExpiration
);

public record UserLoginResponseDto(
    string AccessToken, 
    DateTime AccessTokenExpireDate,
    string RefreshToken,
    DateTime RefreshTokenExpireDate
);

public record AuthMessageResponseDto(
    string Message
);