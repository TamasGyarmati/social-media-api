using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.App.Exceptions;
using SocialMedia.App.Helpers;
using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;

namespace SocialMedia.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController(
    IPostRepository _repo, 
    IImageValidator _imageValidator) : ControllerBase
{
    [HttpGet("all/")]
    public async Task<ActionResult<List<ResponsePostDto>>> GetAllPosts()
    {
        var posts = await _repo.GetAllAsync();

        return posts.Select(x => x.FromDomainToResponsePostDto()).ToList();
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostWithCommentsDto>> GetPostWithCommentsByIdAsync(Guid id)
    {
        var post = await _repo.GetByIdWithCommentsAsync(id);

        if (post is null)
        {
            return NotFound("The post was not found!");
        }
        
        var response = post.FromDomainToPostWithComments();
        
        return Ok(response);
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
        
        string? relativePath;
        try
        {
            relativePath = await _imageValidator.ValidateImageAsync(dto.Image);
        }
        catch (NotAllowedExtensionException e)
        {
            return BadRequest(e.Message);
        }
        
        var post = dto.FromCreatePostToDomain(relativePath, currentUserId);

        var response = await _repo.CreateAsync(post);
        
        return Ok(response.Id);
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
        
        var existingPost = await _repo.GetByIdAsync(id);
        if (existingPost is null)
        {
            return NotFound("Post not found.");
        }
        
        var existingLike = await _repo.GetLikeByIdAsync(existingPost.Id, currentUserId);

        if (existingLike is not null)
        {
            await _repo.DeleteLikeAsync(existingLike);
            return Ok("Post unliked.");
        }

        var like = new PostLike
        {
            PostId = id,
            UserId = currentUserId
        };

        await _repo.CreateLikeAsync(like);
        
        return Ok("Post liked.");
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
        
        var existingPost = await _repo.GetByIdAsync(id);
        
        string? relativePath;
        try
        {
            relativePath = await _imageValidator.ValidateImageAsync(dto.Image);
        }
        catch (NotAllowedExtensionException e)
        {
            return BadRequest(e.Message);
        }
        
        if (existingPost is not null)
        {
            if (existingPost.CreatedById != currentUserId)
            {
                return Unauthorized();
            }
            
            existingPost.UpdateFromDto(dto, relativePath);
            
            await _repo.UpdateAsync(existingPost);

            return Ok(existingPost);
        }
            
        return NotFound("The post was not found!");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeletePostAsync(Guid id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId is null)
        {
            return Unauthorized();
        }
        
        var post = await _repo.GetByIdAsync(id);

        if (post is not null)
        {
            if (post.CreatedById != currentUserId)
            {
                return Unauthorized();
            }
            
            await _repo.DeleteAsync(post);

            return NoContent();
        }
        
        return NotFound("The post was not found!");
    }
}