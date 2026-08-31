using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SocialMedia.App.Exceptions;

namespace SocialMedia.Logic.Services;

public interface IImageValidator
{
    public Task<string?> ProcessAndSaveAvatarAsync(IFormFile? file);
    public Task<string?> ProcessAndSavePostImageAsync(IFormFile? file);
}

public class ImageValidator(IWebHostEnvironment _env) : IImageValidator
{
    readonly string[] ALLOWED_EXTENSIONS = [".jpg", ".jpeg", ".png", ".webp"];
    
    public async Task<string?> ProcessAndSaveAvatarAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        ValidateExtension(file);

        // Avatar: 256x256 négyzetesre vágva a közepétől
        return await ProcessAndSaveAsync(file, "avatars", 256, 256, ResizeMode.Crop);
    }

    public async Task<string?> ProcessAndSavePostImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        ValidateExtension(file);

        // Poszt kép: max 1200 széles vagy 1200 magas, aránytartóan (nem vág le semmit)
        return await ProcessAndSaveAsync(file, "posts", 1200, 1200, ResizeMode.Max);
    }
    
    void ValidateExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!ALLOWED_EXTENSIONS.Contains(extension))
        {
            throw new NotAllowedExtensionException("This image extension is not allowed!");
        }
    }
    
    async Task<string> ProcessAndSaveAsync(
            IFormFile file, 
            string subFolder, 
            int width, 
            int height, 
            ResizeMode resizeMode)
        {
            var uniqueFileName = $"{Guid.NewGuid()}.webp";
    
            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(rootPath, "images", subFolder);
            Directory.CreateDirectory(uploadsFolder);
    
            var fullFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var imageStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);
    
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = resizeMode
            }));
    
            await image.SaveAsWebpAsync(fullFilePath);
    
            return $"/images/{subFolder}/{uniqueFileName}";
        }
}