using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Logic.ReturnResults;
using SocialMedia.Logic.ReturnResults.CommentResults;

namespace SocialMedia.Logic.Logics;

public interface ICommentLogic
{
    public Task<List<CommentResponseDto>> ReadAllFromPostAsync(Guid id);
    public Task<CommentResponseDto?> GetByIdAsync(Guid id);
    public Task<Guid> CreateAsync(CreateCommentRequestDto dto, string currentUserId);
    public Task<CommentLikeToggleResult?> CreateLikeAsync(Guid id, string currentUserId);
    public Task<CommentResult> UpdateAsync(Guid id, UpdateCommentRequestDto dto, string currentUserId);
    public Task<CommentResult> DeleteAsync(Guid id, string currentUserId);
}

public class CommentLogic(ICommentRepository _repo) : ICommentLogic
{
    public async Task<List<CommentResponseDto>> ReadAllFromPostAsync(Guid id)
    {
        var comments = await _repo.GetAllFromPostAsync(id);
        var responseDtos = comments.Select(x => x.FromDomainToCommentResponseDto()).ToList();
        return responseDtos;
    }
    
    public async Task<CommentResponseDto?> GetByIdAsync(Guid id)
    {
        var comment = await _repo.GetByIdAsync(id);
        var response = comment?.FromDomainToCommentResponseDto();
        return response;
    }
    
    public async Task<Guid> CreateAsync(CreateCommentRequestDto dto, string currentUserId)
    {
        var comment = dto.FromCreateCommentToDomain(currentUserId);
        var response = await _repo.CreateAsync(comment);
        return response.Id;
    }
    
    public async Task<CommentLikeToggleResult?> CreateLikeAsync(Guid id, string currentUserId)
    {
        var existingComment = await _repo.GetByIdAsync(id);
        if (existingComment is null)
        {
            return null;
        }
        
        var existingLike = await _repo.GetLikeByIdAsync(existingComment.Id, currentUserId);

        if (existingLike is not null)
        {
            await _repo.DeleteLikeAsync(existingLike);
            return new CommentLikeToggleResult(false, "Comment unliked.");
        }

        var like = new CommentLike
        {
            CommentId = id,
            UserId = currentUserId
        };

        await _repo.CreateLikeAsync(like);
        
        return new CommentLikeToggleResult(true, "Comment liked.");
    }
    
    public async Task<CommentResult> UpdateAsync(Guid id, UpdateCommentRequestDto dto, string currentUserId)
    {
        var existingComment = await _repo.GetByIdAsync(id);

        if (existingComment is null)
        {
            return new CommentResult.NotFound("The comment was not found.");
        }
        
        if (existingComment.CreatedById != currentUserId)
        {
            return new CommentResult.Forbidden("Forbidden.");
        }
            
        existingComment.UpdateFromDto(dto);
            
        await _repo.UpdateAsync(existingComment);
            
        return new CommentResult.Success(existingComment.Id);
    }
    
    public async Task<CommentResult> DeleteAsync(Guid id, string currentUserId)
    {
        var comment = await _repo.GetByIdAsync(id);

        if (comment is null)
        {
            return new CommentResult.NotFound("The comment was not found.");
        }

        if (comment.CreatedById != currentUserId)
        {
            return new CommentResult.Forbidden("Forbidden.");
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
        
        return new CommentResult.Success(null);
    }
}