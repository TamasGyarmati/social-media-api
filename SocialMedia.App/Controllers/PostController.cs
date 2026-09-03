using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Exceptions;
using SocialMedia.Logic.Logics;
using SocialMedia.Logic.ReturnResults.PostResults;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController(IPostLogic _logic) : ControllerBase
{
    [HttpGet("all/")]
    [ProducesResponseType(typeof(List<GetAllPostsResponseDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Fetches all the posts.")]
    public async Task<ActionResult<List<GetAllPostsResponseDto>>> GetAllPosts()
        => Ok(await _logic.ReadAll());
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetPostWithCommentsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Fetches the post by id with the comments included.")]
    public async Task<ActionResult<GetPostWithCommentsResponseDto>> GetPostWithCommentsByIdAsync(Guid id)
    {
        var post = await _logic.ReadWithCommentsByIdAsync(id);
        
        return post is null 
            ? NotFound(new { Message = "Post was not found." }) 
            : Ok(post);
    }
    
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreatePostResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Creates the post from the provided DTO object.")]
    [EndpointDescription("Returns the created entity's ID back.")]
    public async Task<ActionResult<CreatePostResponseDto>> CreatePostAsync([FromForm] CreatePostRequestDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            var id = await _logic.CreateAsync(dto, currentUserId);
            return Ok(new CreatePostResponseDto(id));
        }
        catch (NotAllowedExtensionException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/like")]
    [Authorize]
    [ProducesResponseType(typeof(CreatePostLikeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("Likes the provided post.")]
    public async Task<ActionResult<CreatePostLikeResponseDto>> CreatePostLikeAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.CreateLikeAsync(id, currentUserId);
        
        return result is null 
            ? NotFound(new { Message = "The post was not found." }) 
            : Ok(new CreatePostLikeResponseDto(result.IsLiked, result.Message));
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [Authorize]
    [ProducesResponseType(typeof(UpdatePostResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [EndpointSummary("Updates the provided post.")]
    public async Task<ActionResult<UpdatePostResponseDto>> UpdatePostAsync(Guid id, [FromForm] UpdatePostRequestDto dto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var result = await _logic.UpdateAsync(id, dto, currentUserId);

        return result switch
        {
            PostResult.NotFound error => NotFound(new { error.Message }),
            PostResult.Forbidden error => StatusCode(StatusCodes.Status403Forbidden, new { error.Message }),
            PostResult.Success response => Ok(new { response.Id }),
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
            PostResult.NotFound error => NotFound(new { error.Message }),
            PostResult.Forbidden error => StatusCode(StatusCodes.Status403Forbidden, new { error.Message }),
            PostResult.FailedToDeleteImage error => BadRequest(new { error.Message }),
            PostResult.Success => NoContent(),
            _ => StatusCode(500)
        };
    }
}