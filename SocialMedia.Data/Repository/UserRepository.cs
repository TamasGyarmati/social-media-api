using SocialMedia.Data.Database;

namespace SocialMedia.Data.Repository;

public interface IUserRepository
{
    
}

public class UserRepository(SocialMediaDbContext _db) : IUserRepository
{
    public async Task UploadAvatarAsync()
    {
        
    }
}