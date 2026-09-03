using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.ReturnResults.CommentResults;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController(ICommentLogic _logic) : ControllerBase
{
    [HttpGet("all/{postId:guid}")]
    [ProducesResponseType(typeof(List<CommentResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Fetches all the comments from the provided post.")]
    public async Task<ActionResult<List<CommentResponseDto>>> GetAllCommentFromPostAsync(Guid postId)
        => Ok(await _logic.ReadAllFromPostAsync(postId));
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Fetches the comment by ID.")]
    public async Task<ActionResult<CommentResponseDto>> GetCommentByIdAsync(Guid id)
    {
        var comment = await _logic.GetByIdAsync(id);
        
        return comment is null 
            ? NotFound(new { Message = "Comment was not found."}) 
            : Ok(comment);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateCommentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Creates the comment from the provided DTO object.")]
    [EndpointDescription("Returns the created entity's ID back.")]
    public async Task<ActionResult<CreateCommentResponseDto>> CreateCommentAsync(CreateCommentRequestDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var commentId = await _logic.CreateAsync(dto, currentUserId);
        
        return Ok(new CreateCommentResponseDto(commentId));
    }
    
    [HttpPost("{id:guid}/like")]
    [Authorize]
    [ProducesResponseType(typeof(CreateCommentLikeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Likes the provided comment.")]
    public async Task<ActionResult<CreateCommentLikeResponseDto>> CreateCommentLikeAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.CreateLikeAsync(id, currentUserId);
        
        return result is null 
            ? NotFound(new { Message = "The comment was not found."}) 
            : Ok(new CreateCommentLikeResponseDto(result.IsLiked, result.Message));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(UpdateCommentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EndpointSummary("Updates the provided comment.")]
    public async Task<ActionResult<UpdateCommentResponseDto>> UpdateCommentAsync(Guid id, [FromBody] UpdateCommentRequestDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var result = await _logic.UpdateAsync(id, dto, currentUserId);

        return result switch
        {
            CommentResult.NotFound error => NotFound(new { error.Message }),
            CommentResult.Forbidden error => StatusCode(StatusCodes.Status403Forbidden, new { error.Message }),
            CommentResult.Success response => Ok(new UpdateCommentResponseDto(response.Id)),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EndpointSummary("Deletes the provided comment.")]
    public async Task<IActionResult> DeleteCommentAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var result = await _logic.DeleteAsync(id, currentUserId);

        return result switch
        {
            CommentResult.NotFound error => NotFound(new { error.Message }),
            CommentResult.Forbidden error => StatusCode(StatusCodes.Status403Forbidden, new { error.Message }),
            CommentResult.Success => NoContent(),
            _ => StatusCode(500)
        };
    }
}