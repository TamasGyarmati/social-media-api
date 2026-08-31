using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.App.Exceptions;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Logics;

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
            var avatarUrl = await _logic.UploadAsync(dto, currentUserId);
            
            return avatarUrl is not null 
                ? Ok(new { AvatarUrl = avatarUrl }) 
                : BadRequest("Failed to upload avatar.");
        }
        catch (NotAllowedExtensionException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}