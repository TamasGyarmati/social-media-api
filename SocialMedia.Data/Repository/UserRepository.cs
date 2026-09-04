using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Data.Repository;

public interface IUserRepository
{
    public Task<FollowDbStatus> CreateFollowAsync(string followedId, string followerId, CancellationToken ct = default);
}

public class UserRepository(SocialMediaDbContext _db) : IUserRepository
{
    public async Task<FollowDbStatus> CreateFollowAsync(string followedId, string followerId, CancellationToken ct = default)
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
}