using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Logic.ReturnResults;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IPostLogic
{
    public Task<List<ResponsePostDto>> ReadAll();
    public Task<PostWithCommentsDto?> ReadWithCommentsByIdAsync(Guid id);
    public Task<Guid> CreateAsync(CreatePostDto dto, string currentUserId);
    public Task<LikeToggleResult?> CreateLikeAsync(Guid id, string currentUserId);
    public Task<PostResult> UpdateAsync(Guid id, UpdatePostDto dto, string currentUserId);
    public Task<PostResult> DeleteAsync(Guid id, string currentUserId);
}

public class PostLogic(
    IPostRepository _repo, 
    IImageProcessor _imageProcessor) : IPostLogic
{
    public async Task<List<ResponsePostDto>> ReadAll()
    {
        var posts = await _repo.GetAllAsync();
        return posts.Select(x => x.FromDomainToResponsePostDto()).ToList();
    }
    
    public async Task<PostWithCommentsDto?> ReadWithCommentsByIdAsync(Guid id)
    {
        var post = await _repo.GetByIdWithCommentsAsync(id);
        var dto = post?.FromDomainToPostWithCommentsDto();
        return dto;
    }
    
    public async Task<Guid> CreateAsync(CreatePostDto dto, string currentUserId)
    {
        var relativePath = await _imageProcessor.ProcessAndSavePostImageAsync(dto.Image);
        
        var post = dto.FromCreatePostToDomain(relativePath, currentUserId);
        var response = await _repo.CreateAsync(post);
        
        return response.Id;
    }
    
    public async Task<LikeToggleResult?> CreateLikeAsync(Guid id, string currentUserId)
    {
        var existingPost = await _repo.GetByIdAsync(id);
        if (existingPost is null)
        {
            return null;
        }
        
        var existingLike = await _repo.GetLikeByIdAsync(existingPost.Id, currentUserId);

        if (existingLike is not null)
        {
            await _repo.DeleteLikeAsync(existingLike);
            return new LikeToggleResult(false, "Post unliked.");
        }

        var like = new PostLike
        {
            PostId = id,
            UserId = currentUserId
        };

        await _repo.CreateLikeAsync(like);
        
        return new LikeToggleResult(true, "Post liked.");
    }
    
    public async Task<PostResult> UpdateAsync(Guid id, UpdatePostDto dto, string currentUserId)
    {
        var existingPost = await _repo.GetByIdAsync(id);
        if (existingPost is null)
        {
            return PostResult.NotFound();
        }
        
        if (existingPost.CreatedById != currentUserId)
        {
            return PostResult.Forbidden();
        }
        
        string? relativePath = await _imageProcessor.ProcessAndSavePostImageAsync(dto.Image);
            
        existingPost.UpdateFromDto(dto, relativePath);
        await _repo.UpdateAsync(existingPost);

        return PostResult.Success(existingPost.Id);
    }
    
    public async Task<PostResult> DeleteAsync(Guid id, string currentUserId)
    {
        var post = await _repo.GetByIdAsync(id);

        if (post is null)
        {
            return PostResult.NotFound();
        }

        var oldImage = post.ImageUrl;
        
        if (post.CreatedById != currentUserId)
        {
            return PostResult.Forbidden();
        }
        
        await _repo.DeleteAsync(post);

        if (!string.IsNullOrWhiteSpace(oldImage))
        {
            var result = _imageProcessor.DeleteImage(oldImage);
            if (!result)
            {
                return PostResult.FailedToDeleteImage();
            }
        }

        return PostResult.Success(null);
    }
}