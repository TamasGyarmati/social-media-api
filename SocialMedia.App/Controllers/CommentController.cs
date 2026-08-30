using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Enums;
using SocialMedia.Logic.Logics;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController(ICommentLogic _logic) : ControllerBase
{
    [HttpGet("all/{postId:guid}")]
    public async Task<ActionResult<List<CommentResponseShorterDto>>> GetAllCommentFromPostAsync(Guid id)
        => Ok(await _logic.ReadAllFromPostAsync(id));
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommentResponseDto>> GetCommentByIdAsync(Guid id)
    {
        var comment = await _logic.GetByIdAsync(id);
        return comment is null ? NotFound("Comment was not found.") : Ok(comment);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> CreateCommentAsync(CreateCommentDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var commentId = await _logic.CreateAsync(dto, currentUserId);
        
        return Ok(commentId);
    }
    
    [HttpPost("{id:guid}/like")]
    [Authorize]
    public async Task<ActionResult> CreateCommentLikeAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var message = await _logic.CreateLikeAsync(id, currentUserId);
        
        return message is null ? NotFound("The comment was not found.") : Ok(message);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<Guid>> UpdateCommentAsync(Guid id, [FromBody] UpdateCommentDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var result = await _logic.UpdateAsync(id, dto, currentUserId);

        return result.Status switch
        {
            CommentStatus.NotFound => NotFound("The comment was not found."),
            CommentStatus.Forbidden => Forbid(),
            CommentStatus.Success => Ok(result.CommentId),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteCommentAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var result = await _logic.DeleteAsync(id, currentUserId);

        return result.Status switch
        {
            CommentStatus.NotFound => NotFound("The comment was not found."),
            CommentStatus.Forbidden => Forbid(),
            CommentStatus.Success => NoContent(),
            _ => BadRequest()
        };
    }
}