using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Data.Repository;

public interface IUserRepository
{
    Task<FollowDbStatus> CreateFollowAsync(string followedId, string followerId, CancellationToken ct = default);
    Task<int> SoftDeleteUserAsync(string userId, CancellationToken ct = default);
    Task<int> DeleteFollowsAsync(string userId, CancellationToken ct = default);
}

public class UserRepository(SocialMediaDbContext _db) : IUserRepository
{
    public async Task<FollowDbStatus> CreateFollowAsync(
        string followedId, 
        string followerId, 
        CancellationToken ct = default)
    {
        var targetExists = await _db.AppUsers.AnyAsync(x => x.Id == followedId, ct);
        if (!targetExists)
        {
            return FollowDbStatus.TargetNotFound;
        }

        var alreadyFollowing = await _db.UserFollows
            .AnyAsync(uf => uf.FollowerId == followerId && uf.FollowedId == followedId, ct);
        
        if (alreadyFollowing)
        {
            return FollowDbStatus.AlreadyFollowing;
        }

        var userFollow = new UserFollow
        {
            FollowerId = followerId,
            FollowedId = followedId
        };

        _db.UserFollows.Add(userFollow);

        try
        {
            await _db.SaveChangesAsync(ct);
            return FollowDbStatus.Success;
        }
        catch (DbUpdateException)
        {
            return FollowDbStatus.AlreadyFollowing;
        }
    }

    public async Task<int> SoftDeleteUserAsync(string userId, CancellationToken ct = default)
    {
        return await _db.AppUsers
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.UserName, $"[deleted_user_{userId[..8]}]")
                .SetProperty(u => u.NormalizedUserName, $"[DELETED_USER_{userId[..8]}]")
                .SetProperty(u => u.Email, (string?)null)
                .SetProperty(u => u.NormalizedEmail, (string?)null)
                .SetProperty(u => u.AvatarUrl, (string?)null)
                .SetProperty(u => u.IsDeleted, true)
                .SetProperty(u => u.DeletedAt, DateTime.UtcNow)
                .SetProperty(u => u.FirstName, (string?)null)
                .SetProperty(u => u.LastName, (string?)null)
                .SetProperty(u => u.PhoneNumber, (string?)null)
                .SetProperty(u => u.LockoutEnabled, true)
                .SetProperty(u => u.LockoutEnd, DateTimeOffset.MaxValue)
                .SetProperty(u => u.SecurityStamp, Guid.NewGuid().ToString()),
                ct);
    }

    public async Task<int> DeleteFollowsAsync(string userId, CancellationToken ct = default)
    {
        return await _db.UserFollows
            .Where(uf => uf.FollowedId == userId || uf.FollowerId == userId)
            .ExecuteDeleteAsync(ct);
    }
}