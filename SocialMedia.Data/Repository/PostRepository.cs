using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public class PostRepository(SocialMediaDbContext _db) : IPostRepository
{
    public async Task<List<Post>> GetAllAsync()
        => await _db.Posts
            .AsNoTracking()
            .Include(x => x.Comments)
            .ToListAsync();

    public async Task<Post?> GetByIdAsync(Guid id)
        => await _db.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Post?> GetByIdWithCommentsAsync(Guid id)
        => await _db.Posts
            .AsNoTracking()
            .Include(x => x.Comments)
                .ThenInclude(c => c.Replies)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Post> CreateAsync(Post post)
    {
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        
        return post;
    }

    public async Task<Post> UpdateAsync(Post post)
    {
        _db.Posts.Update(post);
        await _db.SaveChangesAsync();
        
        return post;
    }

    public async Task DeleteAsync(Post post)
    {
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
    }
}