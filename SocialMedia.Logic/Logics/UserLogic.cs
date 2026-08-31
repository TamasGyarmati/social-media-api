using SocialMedia.Data.Repository;
using SocialMedia.Domain.Dtos;
using SocialMedia.Logic.Services;

namespace SocialMedia.Logic.Logics;

public interface IUserLogic
{
    public Task<string?> UploadAsync(UploadAvatarDto dto, string currentUserId);
}

public class UserLogic(
    IUserRepository _repo,
    IImageValidator _imageValidator) : IUserLogic
{
    public async Task<string?> UploadAsync(UploadAvatarDto dto, string currentUserId)
    {
        var relativePath = await _imageValidator.ValidateImageAsync(dto.Image);

        if (relativePath is null)
        {
            return null;
        }
        
        var result = await _repo.UploadAvatarAsync(currentUserId, relativePath);

        return result is 0 ? null : relativePath;
    }
}