using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public interface IPostRepository
{
    public Task<List<Post>> GetAllAsync(CancellationToken ct = default);
    public Task<Post?> GetByIdAsync(Guid id);
    public Task<PostLike?> GetLikeByIdAsync(Guid postId, string userId);
    public Task<Post?> GetByIdWithCommentsAsync(Guid id, CancellationToken ct = default);
    public Task<Post> CreateAsync(Post post, CancellationToken ct = default);
    public Task<PostLike> CreateLikeAsync(PostLike like);
    public Task<Post> UpdateAsync(Post post, CancellationToken ct = default);
    public Task DeleteAsync(Post post);
    public Task DeleteLikeAsync(PostLike like);
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

    public async Task<Post?> GetByIdAsync(Guid id)
        => await _db.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PostLike?> GetLikeByIdAsync(Guid postId, string userId)
        => await _db.PostLikes.FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);

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

    public async Task<PostLike> CreateLikeAsync(PostLike like)
    {
        _db.PostLikes.Add(like);
        await _db.SaveChangesAsync();

        return like;
    }

    public async Task<Post> UpdateAsync(Post post, CancellationToken ct = default)
    {
        _db.Posts.Update(post);
        await _db.SaveChangesAsync(ct);
        
        return post;
    }

    public async Task DeleteAsync(Post post)
    {
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteLikeAsync(PostLike like)
    {
        _db.PostLikes.Remove(like);
        await _db.SaveChangesAsync();
    }
}