using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;
using SocialMedia.Logic.ReturnResults.CommentResults;

namespace SocialMedia.Logic.Logics;

public interface ICommentLogic
{
    Task<List<CommentResponseDto>> ReadAllFromPostAsync(Guid id, CancellationToken ct = default);
    Task<CommentResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateAsync(CreateCommentRequestDto dto, string currentUserId, CancellationToken ct = default);
    Task<CommentLikeToggleResult?> ToggleLikeAsync(Guid id, string currentUserId, CancellationToken ct = default);
    Task<CommentResult> UpdateAsync(Guid id, UpdateCommentRequestDto dto, string currentUserId, CancellationToken ct = default);
    Task<CommentResult> DeleteAsync(Guid id, string currentUserId, CancellationToken ct = default);
}

public class CommentLogic(ICommentRepository _repo) : ICommentLogic
{
    public async Task<List<CommentResponseDto>> ReadAllFromPostAsync(Guid id, CancellationToken ct = default)
    {
        var comments = await _repo.GetAllFromPostAsync(id, ct);
        var responseDtos = comments.Select(x => x.FromDomainToCommentResponseDto()).ToList();
        return responseDtos;
    }
    
    public async Task<CommentResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var comment = await _repo.GetByIdAsync(id, ct);
        var response = comment?.FromDomainToCommentResponseDto();
        return response;
    }
    
    public async Task<Guid> CreateAsync(
        CreateCommentRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var comment = dto.FromCreateCommentToDomain(currentUserId);
        var response = await _repo.CreateAsync(comment, ct);
        return response.Id;
    }
    
    public async Task<CommentLikeToggleResult?> ToggleLikeAsync(
        Guid id, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var existingComment = await _repo.GetByIdAsync(id, ct);
        if (existingComment is null)
        {
            return null;
        }
        
        var existingLike = await _repo.GetLikeByIdAsync(existingComment.Id, currentUserId, ct);

        if (existingLike is not null)
        {
            await _repo.DeleteLikeAsync(existingLike, ct);
            return new CommentLikeToggleResult(false, "Comment unliked.");
        }

        var like = new CommentLike
        {
            CommentId = id,
            UserId = currentUserId
        };

        try
        {
            await _repo.CreateLikeAsync(like, ct);
            return new CommentLikeToggleResult(true, "Comment liked.");
        }
        catch (DbUpdateException)
        {
            return new CommentLikeToggleResult(true, "Comment liked.");
        }
    }
    
    public async Task<CommentResult> UpdateAsync(
        Guid id, 
        UpdateCommentRequestDto dto, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var existingComment = await _repo.GetByIdAsync(id, ct);

        if (existingComment is null)
        {
            return new CommentResult.NotFound("The comment was not found.");
        }
        
        if (existingComment.CreatedById != currentUserId)
        {
            return new CommentResult.Forbidden("Forbidden.");
        }
            
        existingComment.UpdateFromDto(dto);
            
        await _repo.UpdateAsync(existingComment, ct);
            
        return new CommentResult.Success(existingComment.Id);
    }
    
    public async Task<CommentResult> DeleteAsync(
        Guid id, 
        string currentUserId, 
        CancellationToken ct = default)
    {
        var comment = await _repo.GetByIdAsync(id, ct);

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
            await _repo.UpdateAsync(comment, ct);
        }
        else
        {
            await _repo.DeleteAsync(comment, ct);
        }
        
        return new CommentResult.Success(null);
    }
}