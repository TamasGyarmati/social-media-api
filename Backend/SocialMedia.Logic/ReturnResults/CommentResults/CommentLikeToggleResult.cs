namespace SocialMedia.Logic.ReturnResults.CommentResults;

public record CommentLikeToggleResult(
    bool IsLiked, 
    string Message,
    int? Likes = 0
);