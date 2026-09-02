namespace SocialMedia.Logic.ReturnResults.CommentResults;

public record CommentLikeToggleResult(
    bool isLiked, 
    string message
);