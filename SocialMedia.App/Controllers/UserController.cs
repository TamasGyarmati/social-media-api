using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.App.Exceptions;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.ReturnResults.UserResults;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserLogic _logic) : ControllerBase 
{
    [HttpGet]
    [ProducesResponseType(typeof(GetUserByIdResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets the user by ID.")]
    public async Task<ActionResult<GetUserByIdResponseDto>> GetUserById(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.GetUserByIdAsync(id);

        return result switch
        {
            GetUserResult.UserNotFound error => NotFound(new { error.Message }),
            GetUserResult.Success response => Ok(response),
            _ => StatusCode(500)
        };
    }
    
    [HttpPost("avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UploadAvatarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Uploads a user the avatar from the provided DTO.")]
    [EndpointDescription("Returns the relative path of the uploaded Avatar.")]
    public async Task<ActionResult<UploadAvatarResponseDto>> UploadAvatarAsync([FromForm] UploadAvatarRequestDto dto)
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
                UploadAvatarResult.UserNotFound error => NotFound(new { error.Message }),
                UploadAvatarResult.FailedToUpdateAvatar error => BadRequest(new { error.Message }),
                UploadAvatarResult.ImageProcessingError error => BadRequest(new { error.Message }),
                UploadAvatarResult.ExistingAvatarDeleteFailed error => BadRequest(new { error.Message }),
                UploadAvatarResult.Success response => Ok(new UploadAvatarResponseDto(response.RelativePath)),
                _ => StatusCode(500)
            };
        }
        catch (NotAllowedExtensionException ex)
        {
            return BadRequest(new { ex.Message });
        }
    }

    [HttpPut("all")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(UserMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Updates the user through the provided DTO.")]
    public async Task<ActionResult<UserMessageResponseDto>> UpdateUserAsync([FromForm] UpdateUserRequestDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.UpdateAsync(dto, currentUserId);

        return result switch
        {
            UpdateUserResult.AvatarDeletionFailed error => BadRequest(new { error.Message }),
            UpdateUserResult.FailedToUpdateUser error => BadRequest(new { error.Message }),
            UpdateUserResult.UserNameChangeFailed error => BadRequest(new { error.Message }),
            UpdateUserResult.UserNotFound error => NotFound(new { error.Message }),
            UpdateUserResult.Success response => Ok(new UserMessageResponseDto(response.Message)),
            _ => StatusCode(500)
        };
    }
    
    [HttpPut("password")]
    [Authorize]
    [ProducesResponseType(typeof(UserMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Updates the user password through the provided DTO.")]
    public async Task<ActionResult<UserMessageResponseDto>> UpdatePasswordAsync(UpdatePasswordRequestDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.UpdatePasswordAsync(dto, currentUserId);
        
        return result switch
        {
            UpdatePasswordResult.PasswordChangeFailed error => BadRequest(new { error.Message }),
            UpdatePasswordResult.PasswordIsNullOrWhitespace error => BadRequest(new { error.Message }),
            UpdatePasswordResult.UserNotFound error => BadRequest(new { error.Message }),
            UpdatePasswordResult.Success response => Ok(new UserMessageResponseDto(response.Message)),
            _ => StatusCode(500)
        };
    }
    
    [HttpPut("email")]
    [Authorize]
    [ProducesResponseType(typeof(UserMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Requests an email address change through the provided DTO.")]
    public async Task<ActionResult<UserMessageResponseDto>> RequestEmailChange(EmailChangeRequestDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var generatedTokenResult = await _logic.GenerateEmailChangeTokenAsync(dto, currentUserId);
        if (generatedTokenResult is GenerateEmailTokenResult.EmailValidationFailed validationError)
        {
            return BadRequest(new { validationError.Message });
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
            return BadRequest(new { Message = "Failed when making the confirmation link." });
        }

        return result switch
        {
            SendEmailConfirmationResult.EmailOrConfirmationLinkFailed error => BadRequest(new { error.Message }),
            SendEmailConfirmationResult.SendingEmailFailed error => BadRequest(new { error.Message }),
            SendEmailConfirmationResult.Success response => Ok(new UserMessageResponseDto(response.Message)),
            _ => StatusCode(500)
        };
    }

    [HttpGet("confirm-email-change")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Confirms the email through the sent out confirmation link.")]
    public async Task<ActionResult<UserMessageResponseDto>> ConfirmEmailChange(
        [FromQuery] string userId, 
        [FromQuery] string newEmail, 
        [FromQuery] string token)
    {
        var result = await _logic.ConfirmEmailChangeAsync(userId, newEmail, token);
        
        return result switch
        {
            ConfirmEmailChangeResult.ArgumentsRequired error => BadRequest(new { error.Message }),
            ConfirmEmailChangeResult.EmailChangeFailed error => BadRequest(new { error.Message }),
            ConfirmEmailChangeResult.UserNotFound error => NotFound(new { error.Message }),
            ConfirmEmailChangeResult.Success response => Ok( new UserMessageResponseDto(response.Message)),
            _ => StatusCode(500)
        };
    }

    [HttpPost("follow")]
    [Authorize]
    [ProducesResponseType(typeof(FollowUserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Follows the user provided by the ID.")]
    public async Task<ActionResult<FollowUserResponseDto>> FollowUser(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var result = await _logic.CreateFollowAsync(id, currentUserId);

        return result switch
        {
            FollowResult.AlreadyFollowing error => BadRequest(new { error.Message }),
            FollowResult.CannotFollowSelf error => BadRequest(new { error.Message }),
            FollowResult.TargetUserNotFound error => BadRequest(new { error.Message }),
            FollowResult.Success response => Ok(new FollowUserResponseDto(response.Message, id)),
            _ => StatusCode(500)
        };
    }
}