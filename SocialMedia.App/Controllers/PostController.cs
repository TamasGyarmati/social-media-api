using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.App.Exceptions;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Enums;
using SocialMedia.Logic.Logics;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController(IPostLogic _logic) : ControllerBase
{
    [HttpGet("all/")]
    public async Task<ActionResult<List<ResponsePostDto>>> GetAllPosts()
        => Ok(await _logic.ReadAll());
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostWithCommentsDto>> GetPostWithCommentsByIdAsync(Guid id)
    {
        var post = await _logic.ReadWithCommentsByIdAsync(id);
        
        return post is null ? NotFound("Post was not found.") : Ok(post);
    }
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<ActionResult<Guid>> CreatePostAsync([FromForm] CreatePostDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var id = await _logic.CreateAsync(dto, currentUserId);
            return Ok(id);
        }
        catch (NotAllowedExtensionException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/like")]
    [Authorize]
    public async Task<ActionResult> CreatePostLikeAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var message = await _logic.CreateLikeAsync(id, currentUserId);
        
        return message is null ? NotFound("The post was not found.") : Ok(message);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<ActionResult<Guid>> UpdatePostAsync(Guid id, [FromForm] UpdatePostDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.UpdateAsync(id, dto, currentUserId);

        return result.Status switch
        {
            PostStatus.NotFound => NotFound("The post was not found."),
            PostStatus.Forbidden => Forbid(),
            PostStatus.Success => Ok(id),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> DeletePostAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.DeleteAsync(id, currentUserId);

        return result.Status switch
        {
            PostStatus.NotFound => NotFound("The post was not found."),
            PostStatus.Forbidden => Forbid(),
            PostStatus.Success => NoContent(),
            _ => BadRequest()
        };
    }
}