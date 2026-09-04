using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Logic.ReturnResults.PostResults;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IPostLogic
{
    public Task<List<GetAllPostsResponseDto>> ReadAll(CancellationToken ct = default);
    public Task<GetPostWithCommentsResponseDto?> ReadWithCommentsByIdAsync(Guid id, CancellationToken ct = default);
    public Task<Guid> CreateAsync(CreatePostRequestDto dto, string currentUserId, CancellationToken ct = default);
    public Task<PostLikeToggleResult?> ToggleLikeAsync(Guid id, string currentUserId, CancellationToken ct = default);
    public Task<PostResult> UpdateAsync(Guid id, UpdatePostRequestDto dto, string currentUserId, CancellationToken ct = default);
    public Task<PostResult> DeleteAsync(Guid id, string currentUserId, CancellationToken ct = default);
}

public class PostLogic(
    IPostRepository _repo, 
    IImageProcessor _imageProcessor) : IPostLogic
{
    public async Task<List<GetAllPostsResponseDto>> ReadAll(CancellationToken ct = default)
    {
        var posts = await _repo.GetAllAsync(ct);
        return posts.Select(x => x.FromDomainToResponsePostDto()).ToList();
    }
    
    public async Task<GetPostWithCommentsResponseDto?> ReadWithCommentsByIdAsync(Guid id, CancellationToken ct = default)
    {
        var post = await _repo.GetByIdWithCommentsAsync(id, ct);
        var dto = post?.FromDomainToPostWithCommentsDto();
        return dto;
    }
    
    public async Task<Guid> CreateAsync(CreatePostRequestDto dto, string currentUserId, CancellationToken ct = default)
    {
        var relativePath = await _imageProcessor.ProcessAndSavePostImageAsync(dto.Image, ct);
        
        var post = dto.FromCreatePostToDomain(relativePath, currentUserId);
        var response = await _repo.CreateAsync(post, ct);
        
        return response.Id;
    }
    
    public async Task<PostLikeToggleResult?> ToggleLikeAsync(Guid id, string currentUserId, CancellationToken ct = default)
    {
        var existingPost = await _repo.GetByIdAsync(id, ct);
        if (existingPost is null)
        {
            return null;
        }
        
        var existingLike = await _repo.GetLikeByIdAsync(existingPost.Id, currentUserId, ct);

        if (existingLike is not null)
        {
            await _repo.DeleteLikeAsync(existingLike, ct);
            return new PostLikeToggleResult(false, "Post unliked.");
        }

        var like = new PostLike
        {
            PostId = id,
            UserId = currentUserId
        };

        try
        {
            await _repo.CreateLikeAsync(like, ct);
            return new PostLikeToggleResult(true, "Post liked.");
        }
        catch (DbUpdateException)
        {
            // Parallel click caused already a like, so no need to 'unliked'
            return new PostLikeToggleResult(true, "Post liked.");
        }
    }
    
    public async Task<PostResult> UpdateAsync(
        Guid id, 
        UpdatePostRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var existingPost = await _repo.GetByIdAsync(id, ct);
        if (existingPost is null)
        {
            return new PostResult.NotFound("The post was not found.");
        }
        
        if (existingPost.CreatedById != currentUserId)
        {
            return new PostResult.Forbidden("Forbidden.");
        }
        
        var oldImageUrl = existingPost.ImageUrl;
        string? relativePath = null;

        if (dto.Image is not null && dto.Image.Length > 0)
        {
            relativePath = await _imageProcessor.ProcessAndSavePostImageAsync(dto.Image, ct);
            if (relativePath is null)
            {
                return new PostResult.FailedToDeleteImage("Failed to process image");
            }
        }
            
        existingPost.UpdateFromDto(dto, relativePath);

        try
        {
            await _repo.UpdateAsync(existingPost, ct);
        }
        catch (OperationCanceledException)
        {
            if (relativePath is not null)
            {
                _imageProcessor.DeleteImage(relativePath);
            }
            throw;
        }
        catch (Exception)
        {
            if (relativePath is not null)
            {
                _imageProcessor.DeleteImage(relativePath);
            }
            throw;
        }

        if (relativePath is not null && !string.IsNullOrWhiteSpace(oldImageUrl))
        {
            _imageProcessor.DeleteImage(oldImageUrl);
        }

        return new PostResult.Success(existingPost.Id);
    }
    
    public async Task<PostResult> DeleteAsync(Guid id, string currentUserId, CancellationToken ct = default)
    {
        var post = await _repo.GetByIdAsync(id, ct);
        if (post is null)
        {
            return new PostResult.NotFound("The post was not found.");
        }
        
        if (post.CreatedById != currentUserId)
        {
            return new PostResult.Forbidden("Forbidden.");
        }
        
        var oldImage = post.ImageUrl;
        
        await _repo.DeleteAsync(post, ct);

        if (!string.IsNullOrWhiteSpace(oldImage))
        {
            var deleted = _imageProcessor.DeleteImage(oldImage);
            if (!deleted)
            {
                // LOG, by business the image is already deleted, only from the disk it wasn't
            }
        }

        return new PostResult.Success(null);
    }
}