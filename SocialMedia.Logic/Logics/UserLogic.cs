using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Logic.ReturnResults;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IUserLogic
{
    public Task<UploadAvatarResult> UploadAsync(UploadAvatarDto dto, string currentUserId);
    public Task<bool> UpdateAsync(UpdateUserDto dto, string currentUserId);
    public Task<bool> UpdatePasswordAsync(UpdatePasswordDto dto, string currentUserId);
    public Task<string?> GenerateEmailChangeTokenAsync(RequestEmailChangeDto dto, string currentUserId);
    public Task<bool> SendEmailChangeConfirmationAsync(string email, string confirmationLink);
    public Task<bool> ConfirmEmailChangeAsync(string userId, string newEmail, string token);
}

public class UserLogic(
    IImageProcessor _imageProcessor,
    IEmailSender _emailSender,
    UserManager<AppUser> _userManager) : IUserLogic
{
    public async Task<UploadAvatarResult> UploadAsync(UploadAvatarDto dto, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return new UploadAvatarResult.UserNotFound("User was not found.");
        }

        var existingAvatar = user.AvatarUrl;
        if (existingAvatar is not null)
        {
            var response = _imageProcessor.DeleteImage(existingAvatar);
            if (!response)
            {
                return new UploadAvatarResult.ExistingAvatarDeleteFailed("Failed to delete existing avatar.");
            }
        }
        
        var relativePath = await _imageProcessor.ProcessAndSaveAvatarAsync(dto.Image);
        if (relativePath is null)
        {
            return new UploadAvatarResult.ImageProcessingError("Failed to upload avatar.");
        }
        
        user.AvatarUrl = relativePath;

        var result = await _userManager.UpdateAsync(user);
        
        return result.Succeeded 
            ? new UploadAvatarResult.Success(relativePath) 
            : new UploadAvatarResult.FailedToUpdateAvatar("Failed to upload avatar.");
    }

    public async Task<bool> UpdateAsync(UpdateUserDto dto, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return false;
        }
        
        var oldAvatarUrl = user.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
        {
            user.FirstName = dto.FirstName.Trim();
        }
        
        if (!string.IsNullOrWhiteSpace(dto.LastName))
        {
            user.LastName = dto.LastName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.UserName.Trim());
            if (!setUserNameResult.Succeeded)
            {
                return false;
            }
        }

        if (dto.Avatar is not null && dto.Avatar.Length > 0)
        {
            var avatarUrl = await _imageProcessor.ProcessAndSaveAvatarAsync(dto.Avatar);
            if (avatarUrl is not null)
            {
                user.AvatarUrl = avatarUrl;
                
            }
        }
        
        if (!string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            var result = _imageProcessor.DeleteImage(oldAvatarUrl);
            if (!result)
            {
                return false;
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.Succeeded;
    }

    public async Task<bool> UpdatePasswordAsync(UpdatePasswordDto dto, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return false;
        }
        
        var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
        
        return result.Succeeded;
    }
    
    public async Task<string?> GenerateEmailChangeTokenAsync(RequestEmailChangeDto dto, string currentUserId)
    {
        var (found, user) = await ValidateEmailChangeAsync(dto, currentUserId);
        if (!found || user is null)
        {
            return null;
        }

        var rawToken = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail.Trim());
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        return encodedToken;
    }

    async Task<(bool, AppUser?)> ValidateEmailChangeAsync(RequestEmailChangeDto dto, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.NewEmail))
        {
            return (false, null);
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return (false, null);
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.NewEmail);
        
        return existingUser is null 
            ? (true, user)
            : (false, null);
    }

    public async Task<bool> SendEmailChangeConfirmationAsync(string email, string confirmationLink)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(confirmationLink))
        {
            return false;
        }

        var subject = "Confirm Your New Email Address";
        
        var htmlBody = $"""
                        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;">
                            <h2 style="color: #333;">Email Change Request</h2>
                            <p>You recently requested to update the email address associated with your account.</p>
                            <p>Please click the button below to confirm this change:</p>
                            <div style="text-align: center; margin: 30px 0;">
                                <a href="{confirmationLink}" 
                                   style="background-color: #007bff; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: bold; display: inline-block;">
                                    Confirm Email Address
                                </a>
                            </div>
                            <p style="color: #666; font-size: 13px;">If the button above does not work, copy and paste this link into your browser:</p>
                            <p style="word-break: break-all; font-size: 12px; color: #007bff;">{confirmationLink}</p>
                            <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;" />
                            <p style="color: #999; font-size: 12px;">If you did not request this change, please ignore this email or contact support immediately.</p>
                        </div>
                        """;

        try
        {
            await _emailSender.SendEmailAsync(email, subject, htmlBody);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<bool> ConfirmEmailChangeAsync(string userId, string newEmail, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newEmail) ||
            string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        try
        {
            var rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ChangeEmailAsync(user, newEmail, rawToken);
            return result.Succeeded;
        }
        catch
        {
            return false;
        }
    }
}