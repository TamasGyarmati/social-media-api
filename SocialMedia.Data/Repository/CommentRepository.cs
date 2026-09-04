using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public interface ICommentRepository
{
    public Task<List<Comment>> GetAllFromPostAsync(Guid id, CancellationToken ct = default);
    public Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    public Task<CommentLike?> GetLikeByIdAsync(Guid commentId, string userId, CancellationToken ct = default);
    public Task<Comment> CreateAsync(Comment comment, CancellationToken ct = default);
    public Task<CommentLike> CreateLikeAsync(CommentLike like, CancellationToken ct = default);
    public Task<Comment> UpdateAsync(Comment comment, CancellationToken ct = default);
    public Task DeleteAsync(Comment comment, CancellationToken ct = default);
    public Task DeleteLikeAsync(CommentLike commentLike, CancellationToken ct = default);
}

public class CommentRepository(SocialMediaDbContext _db) : ICommentRepository
{
    public async Task<List<Comment>> GetAllFromPostAsync(Guid id, CancellationToken ct = default)
        => await _db.Comments
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => x.PostId == id)
            .Include(x => x.Likes)
            .Include(x => x.Replies)
            .Include(x => x.Creator)
            .ToListAsync(ct);
    
    public async Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Comments
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Likes)
            .Include(x => x.Replies)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<CommentLike?> GetLikeByIdAsync(
        Guid commentId, 
        string userId, 
        CancellationToken ct = default)
        => await _db.CommentLikes.FirstOrDefaultAsync(x => x.CommentId == commentId && x.UserId == userId, ct);

    public async Task<Comment> CreateAsync(Comment comment, CancellationToken ct = default)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);
        
        return comment;
    }

    public async Task<CommentLike> CreateLikeAsync(CommentLike like, CancellationToken ct = default)
    {
        _db.CommentLikes.Add(like);
        await _db.SaveChangesAsync(ct);
        
        return like;
    }

    public async Task<Comment> UpdateAsync(Comment comment, CancellationToken ct = default)
    {
        _db.Comments.Update(comment);
        await _db.SaveChangesAsync(ct);
        
        return comment;
    }

    public async Task DeleteAsync(Comment comment, CancellationToken ct = default)
    {
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteLikeAsync(CommentLike like, CancellationToken ct = default)
    {
        _db.CommentLikes.Remove(like);
        await _db.SaveChangesAsync(ct);
    }
}