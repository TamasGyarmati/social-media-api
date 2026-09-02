using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using SocialMedia.Logic.ReturnResults;
using SocialMedia.Logic.ReturnResults.UserResults;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IUserLogic
{
    public Task<GetUserResult> GetUserByIdAsync(string userId);
    public Task<UploadAvatarResult> UploadAsync(UploadAvatarDto dto, string currentUserId);
    public Task<UpdateUserResult> UpdateAsync(UpdateUserDto dto, string currentUserId);
    public Task<UpdatePasswordResult> UpdatePasswordAsync(UpdatePasswordDto dto, string currentUserId);
    public Task<GenerateEmailTokenResult> GenerateEmailChangeTokenAsync(RequestEmailChangeDto dto, string currentUserId);
    public Task<SendEmailConfirmationResult> SendEmailChangeConfirmationAsync(string email, string confirmationLink);
    public Task<ConfirmEmailChangeResult> ConfirmEmailChangeAsync(string userId, string newEmail, string token);
    public Task<FollowResult> CreateFollowAsync(string targerUserId, string currentUserId);
}

public class UserLogic(
    IImageProcessor _imageProcessor,
    IEmailSender _emailSender,
    IUserRepository _repo,
    UserManager<AppUser> _userManager) : IUserLogic
{
    public async Task<GetUserResult> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.Users
            .AsSplitQuery()
            .Include(u => u.Followers)
                .ThenInclude(f => f.Follower)
            .Include(u => u.Following)
                .ThenInclude(f => f.Followed)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user is null)
        {
            return new GetUserResult.UserNotFound("The user was not found.");
        }
        
        var response = user.FromDomainToGetUserDto();
        
        return new GetUserResult.Success(response);
    }
    
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

    public async Task<UpdateUserResult> UpdateAsync(UpdateUserDto dto, string currentUserId)
    {
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return new UpdateUserResult.UserNotFound("The user was not found.");
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
                return new UpdateUserResult.UserNameChangeFailed("Failed to update username.");
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
                return new UpdateUserResult.AvatarDeletionFailed("Failed to delete avatar.");
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);

        return updateResult.Succeeded 
            ? new UpdateUserResult.Success("User updated.") 
            : new UpdateUserResult.FailedToUpdateUser("Failed to update the user.");
    }

    public async Task<UpdatePasswordResult> UpdatePasswordAsync(UpdatePasswordDto dto, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return new UpdatePasswordResult.PasswordIsNullOrWhitespace("The old password and new password are required.");
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return new UpdatePasswordResult.UserNotFound("The user was not found.");
        }
        
        var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        return result.Succeeded
            ? new UpdatePasswordResult.Success("Password updated.")
            : new UpdatePasswordResult.PasswordChangeFailed("The change of password was unsuccessful");
    }
    
    public async Task<GenerateEmailTokenResult> GenerateEmailChangeTokenAsync(RequestEmailChangeDto dto, string currentUserId)
    {
        var result = await ValidateEmailChangeAsync(dto, currentUserId);
        if (result is ValidateEmailResult.Failure error)
        {
            return new GenerateEmailTokenResult.EmailValidationFailed(error.Message);
        }

        var user = ((ValidateEmailResult.Success)result).User;
        
        var rawToken = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail.Trim());
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        return new GenerateEmailTokenResult.Success(encodedToken);
    }

    async Task<ValidateEmailResult> ValidateEmailChangeAsync(RequestEmailChangeDto dto, string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(dto.NewEmail))
        {
            return new ValidateEmailResult.NewEmailFailed("New email is required.");
        }

        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return new ValidateEmailResult.UserNotFound("The user was not found.");
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.NewEmail);
        
        return existingUser is null 
            ? new ValidateEmailResult.Success(user)
            : new ValidateEmailResult.UserWithEmailExist("User with email address already exists.");
    }

    public async Task<SendEmailConfirmationResult> SendEmailChangeConfirmationAsync(string email, string confirmationLink)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(confirmationLink))
        {
            return new SendEmailConfirmationResult.EmailOrConfirmationLinkFailed("The email or the confirmation link is required.");
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
            return new SendEmailConfirmationResult.Success("Your email address has been sent.");
        }
        catch
        {
            return new SendEmailConfirmationResult.SendingEmailFailed("Something went wrong with sending the email.");
        }
    }
    
    public async Task<ConfirmEmailChangeResult> ConfirmEmailChangeAsync(string userId, string newEmail, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newEmail) ||
            string.IsNullOrWhiteSpace(token))
        {
            return new ConfirmEmailChangeResult.ArgumentsRequired("The User ID, new Email or Token is required.");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new ConfirmEmailChangeResult.UserNotFound("The user was not found.");
        }

        var rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ChangeEmailAsync(user, newEmail, rawToken);
        
        return result.Succeeded
            ? new ConfirmEmailChangeResult.Success("The email was changed.")
            : new ConfirmEmailChangeResult.EmailChangeFailed("The email change failed.");
    }

    public async Task<FollowResult> CreateFollowAsync(string targerUserId, string currentUserId)
    {
        if (string.Equals(targerUserId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            return new FollowResult.CannotFollowSelf("Cannot follow self.");
        }
        
        var dbStatus = await _repo.CreateFollowAsync(targerUserId, currentUserId);
        
        return dbStatus switch
        {
            FollowDbStatus.Success => new FollowResult.Success("Follow was successful!"),
            FollowDbStatus.TargetNotFound => new FollowResult.TargetUserNotFound("Target user not found."),
            FollowDbStatus.AlreadyFollowing => new FollowResult.AlreadyFollowing("Already following."),
            _ => throw new InvalidOperationException("Unhandled DB follow status.")
        };
    }
}