using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public interface ICommentRepository
{
    public Task<List<Comment>> GetAllFromPostAsync(Guid id);
    public Task<Comment?> GetByIdAsync(Guid id);
    public Task<Comment> CreateAsync(Comment comment);
    public Task<Comment> UpdateAsync(Comment comment);
    public Task DeleteAsync(Comment comment);
}