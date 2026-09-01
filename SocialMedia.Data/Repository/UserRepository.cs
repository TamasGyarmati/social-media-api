using Microsoft.EntityFrameworkCore;
using SocialMedia.Data.Database;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Data.Repository;

public interface IUserRepository
{
    public Task<FollowDbStatus> CreateFollowAsync(string followedId, string followerId);
}

public class UserRepository(SocialMediaDbContext _db) : IUserRepository
{
    public async Task<FollowDbStatus> CreateFollowAsync(string followedId, string followerId)
    {
        var targetExists = await _db.AppUsers.AnyAsync(x => x.Id == followedId);
        if (!targetExists)
        {
            return FollowDbStatus.TargetNotFound;
        }

        var alreadyFollowing = await _db.UserFollows
            .AnyAsync(uf => uf.FollowerId == followerId && uf.FollowedId == followedId);
        
        if (alreadyFollowing)
        {
            return FollowDbStatus.AlreadyFollowing;
        }

        var userFollow = new UserFollow
        {
            FollowerId = followerId,
            FollowedId = followedId
        };

        await _db.UserFollows.AddAsync(userFollow);
        await _db.SaveChangesAsync();
        
        return FollowDbStatus.Success;
    }
}