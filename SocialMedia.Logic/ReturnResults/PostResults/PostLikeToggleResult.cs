namespace SocialMedia.Logic.ReturnResults.PostResults;

public record PostLikeToggleResult(
    bool isLiked, 
    string message
);