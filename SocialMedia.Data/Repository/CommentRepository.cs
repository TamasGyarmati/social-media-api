using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public class CommentRepository(SocialMediaDbContext _db) : ICommentRepository
{
    public async Task<List<Comment>> GetAllFromPostAsync(Guid id)
        => await _db.Comments
            .AsNoTracking()
            .Where(x => x.PostId == id)
            .Include(x => x.Replies)
            .ToListAsync();
    
    public async Task<Comment?> GetByIdAsync(Guid id)
        => await _db.Comments
            .AsNoTracking()
            .Include(x => x.Replies)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Comment> CreateAsync(Comment comment)
    {
        await _db.Comments.AddAsync(comment);
        await _db.SaveChangesAsync();
        
        return comment;
    }

    public async Task<Comment> UpdateAsync(Comment comment)
    {
        _db.Comments.Update(comment);
        await _db.SaveChangesAsync();
        
        return comment;
    }

    public async Task DeleteAsync(Comment comment)
    {
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }
}