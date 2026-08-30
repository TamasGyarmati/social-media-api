using Microsoft.AspNetCore.Http;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Domain.Dtos;

public record ResponsePostDto(
    Guid Id,
    string Title,
    string Description,
    string? ImageUrl,
    DateTime CreatedAtUtc,
    string CreatorById,
    int Likes,
    int CommentsCount
);
    
public record PostWithCommentsDto(
    string Title,
    string Description,
    string? ImageUrl,
    DateTime CreatedAtUtc,
    string CreatorById,
    int Likes,
    List<CommentShortDto> Comments
);
    
public record CreatePostDto(
    string Title,
    string Description,
    IFormFile? Image
);

public record UpdatePostDto(
    string Title,
    string Description,
    IFormFile? Image
);

public static class PostMappingExtensions
{
    public static ResponsePostDto FromDomainToResponsePostDto(this Post post)
    {
        return new ResponsePostDto(
            post.Id, 
            post.Title, 
            post.Description, 
            post.ImageUrl,
            post.CreatedAtUtc,
            post.CreatedById,
            post.Likes.Count,
            post.Comments.Count
        );
    }
    
    public static PostWithCommentsDto FromDomainToPostWithCommentsDto(this Post post)
    {
        return new PostWithCommentsDto(
            post.Title,
            post.Description,
            post.ImageUrl,
            post.CreatedAtUtc,
            post.CreatedById,
            post.Likes.Count,
            post.Comments.Select(x => new CommentShortDto(x.Content, x.CreatedAtUtc, x.CreatedById, x.Replies.Count)).ToList()
        );
    }
    
    public static Post FromCreatePostToDomain(this CreatePostDto dto, string? relativePath, string creatorId)
    {
        return new Post
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedById = creatorId,
            ImageUrl = relativePath
        };
    }
    
    public static void UpdateFromDto(this Post post, UpdatePostDto dto, string? relativePath)
    {
        post.Title = dto.Title;
        post.Description = dto.Description;
        post.ImageUrl = relativePath;
    }
}