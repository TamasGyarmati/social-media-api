using Microsoft.AspNetCore.Http;

namespace SocialMedia.Domain.Dtos;

public record UploadAvatarDto(
    IFormFile Image
);