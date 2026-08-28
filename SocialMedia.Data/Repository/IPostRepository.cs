using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public interface IPostRepository
{
    public Task<List<Post>> GetAllAsync();
    public Task<Post?> GetByIdAsync(Guid id);
    public Task<Post?> GetByIdWithCommentsAsync(Guid id);
    public Task<Post> CreateAsync(Post post);
    public Task<Post> UpdateAsync(Post post);
    public Task DeleteAsync(Post post);
}