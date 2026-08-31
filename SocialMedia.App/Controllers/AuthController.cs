using Microsoft.AspNetCore.Mvc;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.ReturnResults;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthLogic _logic) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserCreateDto dto)
    {
        var result = await _logic.RegisterAsync(dto);

        if (result is RegisterResult.Failed failed)
        {
            return BadRequest(failed.Errors);
        }

        if (result is RegisterResult.Success success)
        {
            var confirmationLink = Url.Action(
                action: nameof(ConfirmEmail), // When clicking this URL, the following API endpoint get's called
                controller: "Auth",
                values: new { userId = success.User.Id, token = success.EncodedToken },
                protocol: Request.Scheme
            );

            await _logic.SendConfirmationEmailAsync(success.User.Email!, confirmationLink!);

            return Ok("Register successful. Check your inbox and activate your account.");
        }

        return StatusCode(500);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login(UserLoginDto dto)
    {
        var result = await _logic.LoginAsync(dto);

        return result switch
        {
            LoginResult.EmailUnconfirmed unconfirmed => Unauthorized(unconfirmed.Message),
            LoginResult.UserOrPasswordNotExist dontExist => BadRequest(dontExist.Message),
            LoginResult.Success success => Ok(success.TokenWithExpiryDate),
            _ => StatusCode(500)
        };
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
    {
        var result = await _logic.RefreshAsync(tokenApiDto);
        
        return result switch
        {
            RefreshResult.Invalid invalid => BadRequest(invalid.Message),
            RefreshResult.Success success => Ok(success.Response),
            _ => StatusCode(500)
        };
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _logic.ConfirmEmailAsync(userId, token);
        
        return result switch
        {
            ConfirmEmailResult.InvalidEmailRequest invalidEmail => BadRequest(invalidEmail.Message),
            ConfirmEmailResult.InvalidTokenFormat invalidTokenFormat => BadRequest(invalidTokenFormat.Message),
            ConfirmEmailResult.InvalidTokenOrExpired invalidTokenOrExpired => BadRequest(invalidTokenOrExpired.Message),
            ConfirmEmailResult.UserNotFound userNotFound => NotFound(userNotFound.Message),
            ConfirmEmailResult.Success success => Ok(success.Message),
            _ => StatusCode(500)
        };
    }
}