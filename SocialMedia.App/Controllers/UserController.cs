using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.App.Exceptions;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.ReturnResults;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserLogic _logic) : ControllerBase 
{
    [HttpGet]
    public async Task<ActionResult<GetUserDto>> GetUser(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.GetUserByIdAsync(id);

        return result switch
        {
            GetUserResult.UserNotFound error => NotFound(error.Message),
            GetUserResult.Success response => Ok(response),
            _ => StatusCode(500)
        };
    }
    
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadAvatarAsync([FromForm] UploadAvatarDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        try
        {
            var result = await _logic.UploadAsync(dto, currentUserId);
            
            return result switch
            {
                UploadAvatarResult.UserNotFound error => NotFound(error.Message),
                UploadAvatarResult.FailedToUpdateAvatar error => BadRequest(error.Message),
                UploadAvatarResult.ImageProcessingError error => BadRequest(error.Message),
                UploadAvatarResult.ExistingAvatarDeleteFailed error => BadRequest(error.Message),
                UploadAvatarResult.Success response => Ok(new { AvatarUrl = response.RelativePath }),
                _ => StatusCode(500)
            };
        }
        catch (NotAllowedExtensionException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("all")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UpdateUserAsync([FromForm] UpdateUserDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.UpdateAsync(dto, currentUserId);

        return result switch
        {
            UpdateUserResult.AvatarDeletionFailed error => BadRequest(error.Message),
            UpdateUserResult.FailedToUpdateUser error => BadRequest(error.Message),
            UpdateUserResult.UserNameChangeFailed error => BadRequest(error.Message),
            UpdateUserResult.UserNotFound error => NotFound(error.Message),
            UpdateUserResult.Success response => Ok(response.Message),
            _ => StatusCode(500)
        };
    }
    
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> UpdatePasswordAsync(UpdatePasswordDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.UpdatePasswordAsync(dto, currentUserId);
        
        return result switch
        {
            UpdatePasswordResult.PasswordChangeFailed error => BadRequest(error.Message),
            UpdatePasswordResult.PasswordIsNullOrWhitespace error => BadRequest(error.Message),
            UpdatePasswordResult.UserNotFound error => BadRequest(error.Message),
            UpdatePasswordResult.Success response => Ok(response.Message),
            _ => StatusCode(500)
        };
    }
    
    [HttpPut("email")]
    [Authorize]
    public async Task<IActionResult> RequestEmailChange(RequestEmailChangeDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var generatedTokenResult = await _logic.GenerateEmailChangeTokenAsync(dto, currentUserId);
        if (generatedTokenResult is GenerateEmailTokenResult.EmailValidationFailed validationError)
        {
            return BadRequest(validationError.Message);
        }
        var generatedToken = ((GenerateEmailTokenResult.Success)generatedTokenResult).Token;

        var confirmationLink = Url.Action(
            action: nameof(ConfirmEmailChange),
            controller: "User",
            values: new
            {
                userId = currentUserId,
                newEmail = dto.NewEmail,
                token = generatedToken
            },
            protocol: Request.Scheme
        );

        SendEmailConfirmationResult result;
        if (confirmationLink is not null)
        {
            result = await _logic.SendEmailChangeConfirmationAsync(dto.NewEmail, confirmationLink);  
        }
        else
        {
            return BadRequest("Failed when making the confirmation link.");
        }

        return result switch
        {
            SendEmailConfirmationResult.EmailOrConfirmationLinkFailed error => BadRequest(error.Message),
            SendEmailConfirmationResult.SendingEmailFailed error => BadRequest(error.Message),
            SendEmailConfirmationResult.Success response => Ok(response.Message),
            _ => StatusCode(500)
        };
    }

    [HttpGet("confirm-email-change")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailChange(
        [FromQuery] string userId, 
        [FromQuery] string newEmail, 
        [FromQuery] string token)
    {
        var result = await _logic.ConfirmEmailChangeAsync(userId, newEmail, token);
        
        return result switch
        {
            ConfirmEmailChangeResult.ArgumentsRequired error => BadRequest(error.Message),
            ConfirmEmailChangeResult.EmailChangeFailed error => BadRequest(error.Message),
            ConfirmEmailChangeResult.UserNotFound error => NotFound(error.Message),
            ConfirmEmailChangeResult.Success response => Ok(response.Message),
            _ => StatusCode(500)
        };
    }

    [HttpPost("follow")]
    [Authorize]
    public async Task<IActionResult> FollowUser(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var result = await _logic.CreateFollowAsync(id, currentUserId);

        return result switch
        {
            FollowResult.AlreadyFollowing error => BadRequest(error.Message),
            FollowResult.CannotFollowSelf error => BadRequest(error.Message),
            FollowResult.TargetUserNotFound error => BadRequest(error.Message),
            FollowResult.Success => Ok(new { Message = "Follow was successful!", FollowedId = id }),
            _ => StatusCode(500)
        };
    }
}