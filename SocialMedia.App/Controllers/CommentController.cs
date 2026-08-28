using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController(ICommentRepository _repo) : ControllerBase
{
    [HttpGet("all/{postId:guid}")]
    public async Task<ActionResult<List<CommentResponseShorterDto>>> GetAllCommentFromPostAsync(Guid id)
    {
        var comments = await _repo.GetAllFromPostAsync(id);

        var responseDtos = comments.Select(x => x.FromDomainToCommentResponseShorterDto()).ToList();
            
        return Ok(responseDtos);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommentResponseDto>> GetCommentByIdAsync(Guid id)
    {
        var comment = await _repo.GetByIdAsync(id);
        
        if (comment is null)
        {
            return NotFound("The comment was not found!");
        }

        var response = comment.FromDomainToCommentResponseDto();
        
        return Ok(response);
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
        
        var comment = dto.FromCreateCommentToDomain(currentUserId);
        
        var response = await _repo.CreateAsync(comment);
        
        return Ok(response.Id);
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
        
        var existingComment = await _repo.GetByIdAsync(id);
        if (existingComment is null)
        {
            return NotFound("Comment not found.");
        }
        
        var existingLike = await _repo.GetLikeByIdAsync(existingComment.Id, currentUserId);

        if (existingLike is not null)
        {
            await _repo.DeleteLikeAsync(existingLike);
            return Ok("Post unliked.");
        }

        var like = new CommentLike
        {
            CommentId = id,
            UserId = currentUserId
        };

        await _repo.CreateLikeAsync(like);
        
        return Ok("Post liked.");
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
        
        var existingComment = await _repo.GetByIdAsync(id);

        if (existingComment is not null)
        {
            if (existingComment.CreatedById != currentUserId)
            {
                return Unauthorized();
            }
            
            existingComment.UpdateFromDto(dto);
            
            await _repo.UpdateAsync(existingComment);
            
            return Ok(existingComment.Id);
        }
        
        return NotFound("The comment was not found!");
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
        
        var comment = await _repo.GetByIdAsync(id);

        if (comment is null)
        {
            return NotFound("The comment was not found!");
        }

        if (comment.CreatedById != currentUserId)
        {
            return Unauthorized();
        }

        if (comment.Replies.Count > 0)
        {
            comment.Content = "[This comment was removed by the Author.]";
            comment.DeletedAtUtc = DateTime.UtcNow;
            await _repo.UpdateAsync(comment);
        }
        else
        {
            await _repo.DeleteAsync(comment);
        }
        
        return NoContent();
    }
}