using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;

namespace SocialMedia.Data.Repository;

public interface IUserRepository
{
    public Task<AppUser?> FindUserByIdAsync(string id);
    public Task<int> UploadAvatarAsync(string id, string avatarUrl);
}

public class UserRepository(SocialMediaDbContext _db) : IUserRepository
{
    public async Task<AppUser?> FindUserByIdAsync(string id)
        => await _db.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    
    public async Task<int> UploadAvatarAsync(string id, string avatarUrl)
        => await _db.AppUsers
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.AvatarUrl, avatarUrl));
}