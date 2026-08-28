using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SocialMedia.Domain.Entities;

namespace SocialMedia.App.Helpers;

public interface ITokenGenerator
{
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim>? claims, int expiryInMinutes);
    public Task<string> GenerateRefreshTokenAsync(AppUser user);
}

public class TokenGenerator(
    IConfiguration _config, 
    UserManager<AppUser> _userManager) : ITokenGenerator
{
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "")),
            ValidateLifetime = false
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        
        return principal;
    }
    
    public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim>? claims, int expiryInMinutes)
    {
        var signinKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new Exception("jwt:key not found in appsettings")));

        return new JwtSecurityToken(
            issuer: "socialmedia.com",
            audience: "socialmedia.com",
            claims: claims?.ToArray(),
            expires: DateTime.Now.AddMinutes(expiryInMinutes),
            signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
        );
    }

    public async Task<string> GenerateRefreshTokenAsync(AppUser user)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        string result = Convert.ToBase64String(randomNumber);
        user.RefreshToken = result;
        await _userManager.UpdateAsync(user);
        return result;
    }
}