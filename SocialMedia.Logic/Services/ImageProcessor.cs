using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SocialMedia.Logic.Exceptions;

namespace SocialMedia.Logic.Services;

public interface IImageProcessor
{
    public Task<string?> ProcessAndSaveAvatarAsync(IFormFile? file, CancellationToken ct = default);
    public Task<string?> ProcessAndSavePostImageAsync(IFormFile? file, CancellationToken ct = default);
    public bool DeleteImage(string imageUrl);
}

public class ImageProcessor(IWebHostEnvironment _env) : IImageProcessor
{
    readonly string[] ALLOWED_EXTENSIONS = [".jpg", ".jpeg", ".png", ".webp"];

    public bool DeleteImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return false;
        }

        try
        {
            var relativePath = imageUrl.TrimStart('/', '\\');
            var fullFilePath = Path.Combine(_env.WebRootPath, relativePath);
            var imagesRoot = Path.GetFullPath(Path.Combine(_env.WebRootPath, "images"));
            var targetFile = Path.GetFullPath(fullFilePath);

            if (!targetFile.StartsWith(imagesRoot, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
    
    public async Task<string?> ProcessAndSaveAvatarAsync(IFormFile? file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0) return null;

        ValidateExtension(file);

        // Avatar: 256x256 négyzetesre vágva a közepétől
        return await ProcessAndSaveAsync(file, "avatars", 256, 256, ResizeMode.Crop, ct);
    }

    public async Task<string?> ProcessAndSavePostImageAsync(IFormFile? file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0) return null;

        ValidateExtension(file);

        // Poszt kép: max 1200 széles vagy 1200 magas, aránytartóan (nem vág le semmit)
        return await ProcessAndSaveAsync(file, "posts", 1200, 1200, ResizeMode.Max, ct);
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
            ResizeMode resizeMode,
            CancellationToken ct = default)
        {
            var uniqueFileName = $"{Guid.NewGuid()}.webp";
    
            var rootPath = _env.WebRootPath;
            var uploadsFolder = Path.Combine(rootPath, "images", subFolder);
            Directory.CreateDirectory(uploadsFolder);
    
            var fullFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                await using var imageStream = file.OpenReadStream();
                using var image = await Image.LoadAsync(imageStream, ct);
                
                ct.ThrowIfCancellationRequested();
    
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = resizeMode
                }));
    
                await image.SaveAsWebpAsync(fullFilePath, ct);
    
                return $"/images/{subFolder}/{uniqueFileName}";
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(fullFilePath))
                {
                    try
                    {
                        File.Delete(fullFilePath);
                    }
                    catch
                    {
                        // ?
                    }
                }

                throw;
            }
        }
}