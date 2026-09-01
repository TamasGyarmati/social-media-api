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

    [HttpPut]
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

        if (!result)
        {
            return BadRequest("Failed to update user.");
        } 
        
        return Ok("User updated.");
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
        if (!result)
        {
            return BadRequest("Failed to update password.");
        }
        
        return Ok("Password updated.");
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
        
        var generatedToken = await _logic.GenerateEmailChangeTokenAsync(dto, currentUserId);

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

        if (confirmationLink is not null)
        {
            await _logic.SendEmailChangeConfirmationAsync(dto.NewEmail, confirmationLink);  
        }
        else
        {
            return BadRequest("Failed to request email change.");
        }

        return Ok("Confirmation email sent to you new address.");
    }

    [HttpGet("confirm-email-change")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmailChange(
        [FromQuery] string userId, 
        [FromQuery] string newEmail, 
        [FromQuery] string token)
    {
        var result = await _logic.ConfirmEmailChangeAsync(userId, newEmail, token);
        if (!result)
        {
            return BadRequest("Invalid or expired email change token.");
        }

        return Ok("Email changed successfully. You can now user your email.");
    }
}