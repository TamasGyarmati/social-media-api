using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using SocialMedia.Logic.ReturnResults.AuthResults;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IAuthLogic
{
    public Task<RegisterResult> RegisterAsync(UserCreateRequestDto dto, CancellationToken ct = default);
    public Task<bool> SendConfirmationEmailAsync(string email, string confirmationLink, CancellationToken ct = default);
    public Task<LoginResult> LoginAsync(UserLoginRequestDto dto, CancellationToken ct = default);
    public Task<RefreshResult> RefreshAsync(RefreshRequestDto tokenApiDto, CancellationToken ct = default);
    public Task<ConfirmEmailResult> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default);
}

public class AuthLogic(
    UserManager<AppUser> _userManager, 
    RoleManager<IdentityRole> _roleManager,
    ITokenGenerator _tokenGenerator,
    IEmailSender _emailSender) : IAuthLogic
{
    public async Task<RegisterResult> RegisterAsync(UserCreateRequestDto dto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        
        var user = new AppUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return new RegisterResult.Failed(result.Errors);
        }

        try
        {
            if (await _userManager.Users.CountAsync(ct) == 1)
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
            
            ct.ThrowIfCancellationRequested();
        
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            return new RegisterResult.Success(user, encodedToken);
        }
        catch (OperationCanceledException)
        {
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    public async Task<bool> SendConfirmationEmailAsync(
        string email, 
        string confirmationLink, 
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(confirmationLink))
        {
            return false;
        }
        
        ct.ThrowIfCancellationRequested();

        try
        {
            await _emailSender.SendEmailAsync(
                email: email,
                subject: "Confirm your account",
                htmlMessage: $"""
                              <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                                  <h2 style="color: #333; margin-top: 0;">Welcome to our Community!</h2>
                                  <p style="color: #555; line-height: 1.5;">Thanks for signing up! We're excited to have you on board. Please confirm your email address to activate your account and start exploring.</p>
                                  
                                  <div style="text-align: center; margin: 30px 0;">
                                      <a href="{confirmationLink}" 
                                         style="background-color: #007bff; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; display: inline-block;">
                                          Activate Account
                                      </a>
                                  </div>
                                  
                                  <p style="color: #666; font-size: 13px; line-height: 1.4;">If the button above does not work, copy and paste this link into your browser:</p>
                                  <p style="word-break: break-all; font-size: 12px; color: #007bff;">{confirmationLink}</p>
                                  
                                  <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                                  <p style="color: #999; font-size: 12px; margin-bottom: 0;">If you did not create an account with us, please safely ignore this email.</p>
                              </div>
                              """,
                ct: ct
            );
            
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return true;
        }
    }
    
    public async Task<LoginResult> LoginAsync(UserLoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return new LoginResult.UserOrPasswordNotExist("The user or password doesn't exist.");
        }
        
        ct.ThrowIfCancellationRequested();
            
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            return new LoginResult.UserOrPasswordNotExist("The user or password doesn't exist.");
        }
        
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return new LoginResult.EmailUnconfirmed("Please confirm your email before logging in.");
        }
        
        ct.ThrowIfCancellationRequested();
                
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
        var refreshToken = _tokenGenerator.GenerateRefreshToken(user, refreshTokenExpiryInMinutes);
        
        ct.ThrowIfCancellationRequested();
        
        await _userManager.UpdateAsync(user);
                
        var tokenWithExpiryDate = new UserLoginResponseDto(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(accessToken),
            AccessTokenExpireDate: accessToken.ValidTo, 
            RefreshToken: refreshToken,
            RefreshTokenExpireDate: user.RefreshTokenExpiryTime!.Value);
                
        return new LoginResult.Success(tokenWithExpiryDate);
    }
    
    public async Task<RefreshResult> RefreshAsync(RefreshRequestDto tokenApiDto, CancellationToken ct = default)
    {
        string accessToken = tokenApiDto.AccessToken;
        string refreshToken = tokenApiDto.RefreshToken;
        
        var principal = _tokenGenerator.GetPrincipalFromExpiredToken(accessToken);

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return new RefreshResult.Invalid("Invalid client request");
        }
        
        var user = await _userManager.GetUserAsync(principal);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new RefreshResult.Invalid("Invalid client request");   
        }
        
        ct.ThrowIfCancellationRequested();

        const int accessTokenExpiryInMinutes = 24 * 60;
        const int refreshTokenExpiryInMinutes = 24 * 60 * 7;
        
        var newAccessToken = _tokenGenerator.GenerateAccessToken(principal.Claims, accessTokenExpiryInMinutes);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken(user, refreshTokenExpiryInMinutes);
        
        ct.ThrowIfCancellationRequested();
        
        await _userManager.UpdateAsync(user);

        return new RefreshResult.Success(new RefreshResponseDto(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            RefreshToken: newRefreshToken,
            AccessTokenExpiration: newAccessToken.ValidTo,
            RefreshTokenExpiration: user.RefreshTokenExpiryTime!.Value
        ));
    }
    
    public async Task<ConfirmEmailResult> ConfirmEmailAsync(string userId, string token, CancellationToken ct = default)
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
        
        ct.ThrowIfCancellationRequested();

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