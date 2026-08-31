namespace SocialMedia.Logic.ReturnResults;

public record LikeToggleResult(
    bool isLiked, 
    string message
);