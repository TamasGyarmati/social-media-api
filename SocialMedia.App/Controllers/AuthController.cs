using Microsoft.AspNetCore.Authorization;
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
                action: nameof(ConfirmEmail),
                controller: "Auth",
                values: new { userId = success.User.Id, token = success.EncodedToken },
                protocol: Request.Scheme
            );

            var response = await _logic.SendConfirmationEmailAsync(success.User.Email!, confirmationLink!);
            if (!response)
            {
                return BadRequest("An error occurred while sending the confirmation.");
            }

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
            LoginResult.EmailUnconfirmed error => Unauthorized(error.Message),
            LoginResult.UserOrPasswordNotExist error => BadRequest(error.Message),
            LoginResult.Success response => Ok(response.TokenWithExpiryDate),
            _ => StatusCode(500)
        };
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenApiDto tokenApiDto)
    {
        var result = await _logic.RefreshAsync(tokenApiDto);
        
        return result switch
        {
            RefreshResult.Invalid error => BadRequest(error.Message),
            RefreshResult.Success response => Ok(response.Response),
            _ => StatusCode(500)
        };
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await _logic.ConfirmEmailAsync(userId, token);
        
        return result switch
        {
            ConfirmEmailResult.InvalidEmailRequest error => BadRequest(error.Message),
            ConfirmEmailResult.InvalidTokenFormat error => BadRequest(error.Message),
            ConfirmEmailResult.InvalidTokenOrExpired error => BadRequest(error.Message),
            ConfirmEmailResult.UserNotFound error => NotFound(error.Message),
            ConfirmEmailResult.Success response => Ok(response.Message),
            _ => StatusCode(500)
        };
    }
}