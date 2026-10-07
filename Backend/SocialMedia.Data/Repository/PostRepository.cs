using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Dtos;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public interface IPostRepository
{
    Task<List<Post>> GetAllAsync(CancellationToken ct = default);
    Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PostLike?> GetLikeByIdAsync(Guid postId, string userId, CancellationToken ct = default);
    Task<Post?> GetByIdWithCommentsAsync(Guid id, CancellationToken ct = default);
    Task<Post> CreateAsync(Post post, CancellationToken ct = default);
    Task<PostLike> CreateLikeAsync(PostLike like, CancellationToken ct = default);
    Task<int> UpdateAsync(Guid postId, UpdatePostRequestDto dto, string? imageUrl, CancellationToken ct = default);
    Task DeleteAsync(Guid postId, CancellationToken ct = default);
    Task DeleteLikeAsync(Guid postId, string userId, CancellationToken ct = default);
}

public class PostRepository(SocialMediaDbContext _db) : IPostRepository
{
    public async Task<List<Post>> GetAllAsync(CancellationToken ct = default)
        => await _db.Posts
            .AsNoTracking() // Useful when not using SaveChanges (Get), turns off tracking
            .AsSplitQuery()
            .Include(x => x.Comments)
            .Include(p => p.Likes)
            .ToListAsync(ct);
    
    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<PostLike?> GetLikeByIdAsync(Guid postId, string userId, CancellationToken ct = default)
        => await _db.PostLikes.FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId, ct);

    public async Task<Post?> GetByIdWithCommentsAsync(Guid id, CancellationToken ct = default)
        => await _db.Posts
            .AsNoTracking()
            .AsSplitQuery() // More, faster SELECT instead of one big JOIN
            .Include(x => x.Likes)
            .Include(x => x.Comments)
                .ThenInclude(c => c.Replies)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Post> CreateAsync(Post post, CancellationToken ct = default)
    {
        _db.Posts.Add(post);
        await _db.SaveChangesAsync(ct);
        
        return post;
    }

    public async Task<PostLike> CreateLikeAsync(PostLike like, CancellationToken ct = default)
    {
        _db.PostLikes.Add(like);
        await _db.SaveChangesAsync(ct);

        return like;
    }

    public async Task<int> UpdateAsync(Guid postId, UpdatePostRequestDto dto, string? imageUrl, CancellationToken ct = default)
    {
        return await _db.Posts
            .Where(x => x.Id == postId)
            .ExecuteUpdateAsync(setters =>
            {
                setters.SetProperty(x => x.Title, dto.Title)
                    .SetProperty(x => x.Description, dto.Description)
                    .SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow);

                if (imageUrl is not null)
                {
                    setters.SetProperty(x => x.ImageUrl, imageUrl);
                }
            }, ct);
    }

    public async Task DeleteAsync(Guid postId, CancellationToken ct = default)
        => await _db.Posts.Where(x => x.Id == postId).ExecuteDeleteAsync(ct);

    public async Task DeleteLikeAsync(Guid postId, string userId, CancellationToken ct = default)
        => await _db.PostLikes.Where(x => x.PostId == postId && x.UserId == userId).ExecuteDeleteAsync(ct);
}