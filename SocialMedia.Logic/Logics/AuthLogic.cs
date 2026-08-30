using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Logic.Helpers;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IAuthLogic
{
    public Task<RegisterResult> RegisterAsync(UserCreateDto dto);
    public Task SendConfirmationEmailAsync(string email, string confirmationLink);
    public Task<LoginResult> LoginAsync(UserLoginDto dto);
    public Task<RefreshResult> RefreshAsync(TokenApiDto tokenApiDto);
    public Task<ConfirmEmailResult> ConfirmEmailAsync(string userId, string token);
}

public class AuthLogic(UserManager<AppUser> _userManager, 
    RoleManager<IdentityRole> _roleManager,
    ITokenGenerator _tokenGenerator,
    IEmailSender _emailSender)
{
    public async Task<RegisterResult> RegisterAsync(UserCreateDto dto)
    {
        var user = new AppUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            RefreshToken = "",
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return new RegisterResult.Failed(result.Errors);
        }
        
        if (_userManager.Users.Count() == 1)
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");
        }
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        return new RegisterResult.Success(user, encodedToken);
    }

    public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        => await _emailSender.SendEmailAsync(
            email: email,
            subject: "Confirm your account",
            htmlMessage: $"""
                          <h2>Hey, welcome to our website!</h2>
                          <p>Please click the button below to activate your account:</p>
                          <p>
                              <a href='{confirmationLink}' 
                                 style='display:inline-block; padding:10px 20px; background-color:#007bff; color:#ffffff; text-decoration:none; border-radius:5px; font-weight:bold;'>
                                 Activate Account
                              </a>
                          </p>
                          """
        );
    
    public async Task<LoginResult> LoginAsync(UserLoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user != null)
        {
            var result = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (result)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return new LoginResult.EmailUnconfirmed("Please confirm your email before logging in.");
                }
                
                var claim = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName!),
                    new(ClaimTypes.NameIdentifier, user.Id)
                };

                foreach (var role in await _userManager.GetRolesAsync(user))
                {
                    claim.Add(new Claim(ClaimTypes.Role, role));
                }

                const int accessTokenExpiryInMinutes = 24 * 60;
                var accessToken = _tokenGenerator.GenerateAccessToken(claim, accessTokenExpiryInMinutes);
                const int refreshTokenExpiryInMinutes = 24 * 60 * 7;
                var refreshToken = await _tokenGenerator.GenerateRefreshTokenAsync(user);
                
                var tokenWithExpiryDate = new LoginResultDto(
                    AccessToken: new JwtSecurityTokenHandler().WriteToken(accessToken),
                    AccessTokenExpireDate: DateTime.Now.AddMinutes(accessTokenExpiryInMinutes), 
                    RefreshToken: refreshToken, 
                    RefreshTokenExpireDate: DateTime.Now.AddMinutes(refreshTokenExpiryInMinutes));
                
                return new LoginResult.Success(tokenWithExpiryDate);
            }
        }

        return new LoginResult.UserOrPasswordNotExist("The user or password doesn't exist.");
    }
    
    public async Task<RefreshResult> RefreshAsync(TokenApiDto tokenApiDto)
    {
        string accessToken = tokenApiDto.AccessToken;
        string refreshToken = tokenApiDto.RefreshToken;
        
        var principal = _tokenGenerator.GetPrincipalFromExpiredToken(accessToken);

        var user = await _userManager.GetUserAsync(principal);
        
        if (user is null || user.RefreshToken != refreshToken)
            return new RefreshResult.Invalid("Invalid client request");

        const int accessTokenExpiryInMinutes = 24 * 60;
        
        var newAccessToken = _tokenGenerator.GenerateAccessToken(principal?.Claims, accessTokenExpiryInMinutes);
        var newRefreshToken = await _tokenGenerator.GenerateRefreshTokenAsync(user);
        user.RefreshToken = newRefreshToken;

        await _userManager.UpdateAsync(user);

        return new RefreshResult.Success(new AuthResponseDto(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            RefreshToken: newRefreshToken,
            AccessTokenExpiration: newAccessToken.ValidTo,
            RefreshTokenExpiration: DateTime.Now.AddMinutes(accessTokenExpiryInMinutes * 7)
        ));
    }
    
    public async Task<ConfirmEmailResult> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return new ConfirmEmailResult.InvalidEmailRequest("Invalid email confirmation request.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new ConfirmEmailResult.UserNotFound("User not found.");
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return new ConfirmEmailResult.InvalidTokenFormat("Invalid token format.");
        }
        
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            return new ConfirmEmailResult.Success("Email confirmed successfully. You can now log in.");
        }
        
        return new ConfirmEmailResult.InvalidTokenOrExpired("The confirmation token is invalid or has expired.");
    }
}