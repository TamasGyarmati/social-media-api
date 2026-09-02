using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.App.Exceptions;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Enums;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.ReturnResults;
using SocialMedia.Logic.ReturnResults.PostResults;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController(IPostLogic _logic) : ControllerBase
{
    [HttpGet("all/")]
    [ProducesResponseType(typeof(List<ResponsePostDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Fetches all the posts available.")]
    public async Task<ActionResult<List<ResponsePostDto>>> GetAllPosts()
        => Ok(await _logic.ReadAll());
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostWithCommentsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Fetches the post by id with the comments included.")]
    public async Task<ActionResult<PostWithCommentsDto>> GetPostWithCommentsByIdAsync(Guid id)
    {
        var post = await _logic.ReadWithCommentsByIdAsync(id);
        
        return post is null ? NotFound("Post was not found.") : Ok(post);
    }
    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Creates the post from the provided DTO object. Returns the created entity's ID back.")]
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
    [ProducesResponseType(typeof(PostLikeToggleResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Likes the provided post.")]
    public async Task<ActionResult<PostLikeToggleResult>> CreatePostLikeAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.CreateLikeAsync(id, currentUserId);
        
        return result is null ? NotFound("The post was not found.") : Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EndpointSummary("Updates the provided post.")]
    public async Task<ActionResult<Guid>> UpdatePostAsync(Guid id, [FromForm] UpdatePostDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.UpdateAsync(id, dto, currentUserId);

        return result switch
        {
            PostResult.NotFound error => NotFound(error.Message),
            PostResult.Forbidden error => StatusCode(StatusCodes.Status403Forbidden, error.Message),
            PostResult.Success response => Ok(response.Id),
            _ => StatusCode(500)
        };
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EndpointSummary("Deletes the provided post.")]
    public async Task<IActionResult> DeletePostAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.DeleteAsync(id, currentUserId);

        return result switch
        {
            PostResult.NotFound error => NotFound(error.Message),
            PostResult.Forbidden error => StatusCode(StatusCodes.Status403Forbidden, error.Message),
            PostResult.FailedToDeleteImage error => BadRequest(error.Message),
            PostResult.Success => NoContent(),
            _ => StatusCode(500)
        };
    }
}