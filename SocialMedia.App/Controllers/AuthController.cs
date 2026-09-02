using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.ReturnResults.AuthResults;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthLogic _logic) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Creates a new user by the provided DTO.")]
    [EndpointDescription("Sends out a confirmation link in the provided email.")]
    public async Task<IActionResult> Register(UserCreateDto dto)
    {
        var result = await _logic.RegisterAsync(dto);

        switch (result)
        {
            case RegisterResult.Failed error:
                return BadRequest(error.Errors);
            
            case RegisterResult.Success response:
            {
                var confirmationLink = Url.Action(
                    action: nameof(ConfirmEmail),
                    controller: "Auth",
                    values: new { userId = response.User.Id, token = response.EncodedToken },
                    protocol: Request.Scheme
                );

                var confirmed = await _logic.SendConfirmationEmailAsync(response.User.Email!, confirmationLink!);
                if (!confirmed)
                {
                    return BadRequest(new { Message = "An error occurred while sending the confirmation."});
                }

                return Ok(new { Message = "Register successful. Check your inbox and activate your account." });
            }
            
            default:
                return StatusCode(500);
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Logs in the user.")]
    [EndpointDescription("Returns the access/refresh tokens with their expiry date.")]
    public async Task<ActionResult<LoginResultDto>> Login(UserLoginDto dto)
    {
        var result = await _logic.LoginAsync(dto);

        return result switch
        {
            LoginResult.EmailUnconfirmed error => StatusCode(StatusCodes.Status403Forbidden, new { error.Message }),
            LoginResult.UserOrPasswordNotExist error => Unauthorized( new { error.Message }),
            LoginResult.Success response => Ok(response.TokenWithExpiryDate),
            _ => StatusCode(500)
        };
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Refreshes the token.")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(TokenApiDto tokenApiDto)
    {
        var result = await _logic.RefreshAsync(tokenApiDto);
        
        return result switch
        {
            RefreshResult.Invalid error => Unauthorized(new { error.Message }),
            RefreshResult.Success response => Ok(response.Response),
            _ => StatusCode(500)
        };
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Confirms the email through the sent out confirmation link.")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _logic.ConfirmEmailAsync(userId, token);
        
        return result switch
        {
            ConfirmEmailResult.InvalidEmailRequest error => BadRequest(new { error.Message }),
            ConfirmEmailResult.InvalidTokenFormat error => BadRequest(new { error.Message }),
            ConfirmEmailResult.InvalidTokenOrExpired error => BadRequest(new { error.Message }),
            ConfirmEmailResult.UserNotFound error => NotFound(new { error.Message }),
            ConfirmEmailResult.Success response => Ok(new { response.Message }),
            _ => StatusCode(500)
        };
    }
}