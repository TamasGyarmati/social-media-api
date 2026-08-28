using SocialMedia.App.Exceptions;

namespace SocialMedia.App.Helpers;

public interface IImageValidator
{
    public Task<string?> ValidateImageAsync(IFormFile? image);
}

public class ImageValidator(IWebHostEnvironment _env) : IImageValidator
{
    readonly string[] ALLOWED_EXTENSIONS = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<string?> ValidateImageAsync(IFormFile? image)
    {
        if (image is not null && image.Length > 0)
        {
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!ALLOWED_EXTENSIONS.Contains(extension))
            {
                throw new NotAllowedExtensionException("This image extension is not allowed!");
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            
            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(rootPath, "images");
            Directory.CreateDirectory(uploadsFolder);
            
            var fullFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/images/{uniqueFileName}";
        }

        return null;
    }
}