using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;
using SocialMedia.Logic.ReturnResults.UserResults;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IUserLogic
{
    Task<GetUserResult> GetUserByIdAsync(string userId, CancellationToken ct = default);
    Task<UploadAvatarResult> UploadAsync(UploadAvatarRequestDto dto, string currentUserId, CancellationToken ct = default);
    Task<UpdateUserResult> UpdateAsync(UpdateUserRequestDto dto, string currentUserId, CancellationToken ct = default);
    Task<UpdatePasswordResult> UpdatePasswordAsync(UpdatePasswordRequestDto dto, string currentUserId, CancellationToken ct = default);
    Task<GenerateEmailTokenResult> GenerateEmailChangeTokenAsync(EmailChangeRequestDto dto, string currentUserId, CancellationToken ct = default);
    Task<SendEmailConfirmationResult> SendEmailChangeConfirmationAsync(string email, string confirmationLink, CancellationToken ct = default);
    Task<ConfirmEmailChangeResult> ConfirmEmailChangeAsync(string userId, string newEmail, string token, CancellationToken ct = default);
    Task<FollowResult> CreateFollowAsync(string targerUserId, string currentUserId, CancellationToken ct = default);
}

public class UserLogic(
    IImageProcessor _imageProcessor,
    IEmailSender _emailSender,
    IUserRepository _repo,
    UserManager<AppUser> _userManager) : IUserLogic
{
    public async Task<GetUserResult> GetUserByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.Users
            .AsSplitQuery()
            .Include(u => u.Followers)
                .ThenInclude(f => f.Follower)
            .Include(u => u.Following)
                .ThenInclude(f => f.Followed)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        
        if (user is null)
        {
            return new GetUserResult.UserNotFound("The user was not found.");
        }
        
        var response = user.FromDomainToGetUserDto();
        
        return new GetUserResult.Success(response);
    }
    
    public async Task<UploadAvatarResult> UploadAsync(
        UploadAvatarRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return new UploadAvatarResult.UserNotFound("User was not found.");
        }
        
        var oldAvatar = user.AvatarUrl;
        
        var relativePath = await _imageProcessor.ProcessAndSaveAvatarAsync(dto.Image, ct);
        if (relativePath is null)
        {
            return new UploadAvatarResult.ImageProcessingError("Failed to upload avatar.");
        }
        
        user.AvatarUrl = relativePath;

        IdentityResult result;
        try
        {
            result = await _userManager.UpdateAsync(user);
        }
        catch (OperationCanceledException)
        {
            _imageProcessor.DeleteImage(relativePath);
            throw; // need to throw it but aspnet wont throw the 500
        }
        
        if (!result.Succeeded)
        {
            _imageProcessor.DeleteImage(relativePath);
            return new UploadAvatarResult.FailedToUpdateAvatar("Failed to upload avatar.");
        }
        
        if (oldAvatar is not null)
        {
            _imageProcessor.DeleteImage(oldAvatar);
        }
        
        return new UploadAvatarResult.Success(relativePath);
    }

    public async Task<UpdateUserResult> UpdateAsync(
        UpdateUserRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(currentUserId);
        if (user is null)
        {
            return new UpdateUserResult.UserNotFound("The user was not found.");
        }
        
        var oldAvatarUrl = user.AvatarUrl;
        string? newAvatarUrl = null;

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
            newAvatarUrl = await _imageProcessor.ProcessAndSaveAvatarAsync(dto.Avatar, ct);
            if (newAvatarUrl is null)
            {
                return new UpdateUserResult.FailedToUpdateUser("Failed to process avatar.");
            }
            user.AvatarUrl = newAvatarUrl;
        }

        IdentityResult updateResult;
        try
        {
            updateResult = await _userManager.UpdateAsync(user);
        }
        catch (OperationCanceledException)
        {
            if (newAvatarUrl is not null)
            {
                _imageProcessor.DeleteImage(newAvatarUrl);
            }
            throw;
        }

        if (!updateResult.Succeeded)
        {
            if (newAvatarUrl is not null)
            {
                _imageProcessor.DeleteImage(newAvatarUrl);
            }
            return new UpdateUserResult.FailedToUpdateUser("Failed to update the user.");
        }

        if (newAvatarUrl is not null && !string.IsNullOrWhiteSpace(oldAvatarUrl))
        {
            _imageProcessor.DeleteImage(oldAvatarUrl);
        }

        return new UpdateUserResult.Success("User updated.");
    }

    public async Task<UpdatePasswordResult> UpdatePasswordAsync(
        UpdatePasswordRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
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
        
        ct.ThrowIfCancellationRequested();
        
        var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

        return result.Succeeded
            ? new UpdatePasswordResult.Success("Password updated.")
            : new UpdatePasswordResult.PasswordChangeFailed("The change of password was unsuccessful");
    }
    
    public async Task<GenerateEmailTokenResult> GenerateEmailChangeTokenAsync(
        EmailChangeRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var result = await ValidateEmailChangeAsync(dto, currentUserId, ct);
        if (result is ValidateEmailResult.Failure error)
        {
            return new GenerateEmailTokenResult.EmailValidationFailed(error.Message);
        }

        var user = ((ValidateEmailResult.Success)result).User;
        
        ct.ThrowIfCancellationRequested();
        
        var rawToken = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail.Trim());
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        return new GenerateEmailTokenResult.Success(encodedToken);
    }

    async Task<ValidateEmailResult> ValidateEmailChangeAsync(
        EmailChangeRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
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
        
        ct.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(dto.NewEmail);
        
        return existingUser is null 
            ? new ValidateEmailResult.Success(user)
            : new ValidateEmailResult.UserWithEmailExist("User with email address already exists.");
    }

    public async Task<SendEmailConfirmationResult> SendEmailChangeConfirmationAsync(
        string email, 
        string confirmationLink, 
        CancellationToken ct = default)
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

        ct.ThrowIfCancellationRequested();

        try
        {
            await _emailSender.SendEmailAsync(email, subject, htmlBody, ct);
            return new SendEmailConfirmationResult.Success("Your email address has been sent.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return new SendEmailConfirmationResult.SendingEmailFailed("Something went wrong with sending the email.");
        }
    }
    
    public async Task<ConfirmEmailChangeResult> ConfirmEmailChangeAsync(
        string userId, 
        string newEmail, 
        string token, 
        CancellationToken ct = default)
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
        
        ct.ThrowIfCancellationRequested();

        string rawToken;
        try
        {
            rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return new ConfirmEmailChangeResult.EmailChangeFailed("The email change failed.");
        }
        
        var result = await _userManager.ChangeEmailAsync(user, newEmail, rawToken);
        
        return result.Succeeded
            ? new ConfirmEmailChangeResult.Success("The email was changed.")
            : new ConfirmEmailChangeResult.EmailChangeFailed("The email change failed.");
    }

    public async Task<FollowResult> CreateFollowAsync(
        string targerUserId, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        if (string.Equals(targerUserId, currentUserId, StringComparison.OrdinalIgnoreCase))
        {
            return new FollowResult.CannotFollowSelf("Cannot follow self.");
        }
        
        var dbStatus = await _repo.CreateFollowAsync(targerUserId, currentUserId, ct);
        
        return dbStatus switch
        {
            FollowDbStatus.Success => new FollowResult.Success("Follow was successful!"),
            FollowDbStatus.TargetNotFound => new FollowResult.TargetUserNotFound("Target user not found."),
            FollowDbStatus.AlreadyFollowing => new FollowResult.AlreadyFollowing("Already following."),
            _ => throw new InvalidOperationException("Unhandled DB follow status.")
        };
    }
}