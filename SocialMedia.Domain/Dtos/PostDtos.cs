using Microsoft.AspNetCore.Http;
using SocialMedia.Domain.Entities;
using SocialMedia.Domain.Enums;

namespace SocialMedia.Domain.Dtos;

public record GetAllPostsResponseDto(
    Guid Id,
    string Title,
    string Description,
    string? ImageUrl,
    DateTime CreatedAtUtc,
    string CreatorById,
    int Likes,
    int CommentsCount
);
    
public record GetPostWithCommentsResponseDto(
    Guid Id,
    string Title,
    string Description,
    string? ImageUrl,
    DateTime CreatedAtUtc,
    string CreatorById,
    int Likes,
    List<CommentShortDto> Comments
);
    
public record CreatePostRequestDto(
    string Title,
    string Description,
    IFormFile? Image
);

public record UpdatePostRequestDto(
    string Title,
    string Description,
    IFormFile? Image
);

public record CreatePostResponseDto(
    Guid Id
);

public record UpdatePostResponseDto(
    Guid Id
);

public record CreatePostLikeResponseDto(
    bool IsLiked, 
    string Message
);

public static class PostMappingExtensions
{
    public static GetAllPostsResponseDto FromDomainToResponsePostDto(this Post post)
    {
        return new GetAllPostsResponseDto(
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
    
    public static GetPostWithCommentsResponseDto FromDomainToPostWithCommentsDto(this Post post)
    {
        return new GetPostWithCommentsResponseDto(
            post.Id,
            post.Title,
            post.Description,
            post.ImageUrl,
            post.CreatedAtUtc,
            post.CreatedById,
            post.Likes.Count,
            post.Comments.Select(x => new CommentShortDto(x.Content, x.CreatedAtUtc, x.CreatedById, x.Replies.Count)).ToList()
        );
    }
    
    public static Post FromCreatePostToDomain(this CreatePostRequestDto dto, string? relativePath, string creatorId)
    {
        return new Post
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedById = creatorId,
            ImageUrl = relativePath
        };
    }
    
    public static void UpdateFromDto(this Post post, UpdatePostRequestDto dto, string? relativePath)
    {
        post.Title = dto.Title;
        post.Description = dto.Description;
        post.ImageUrl = relativePath;
    }
}