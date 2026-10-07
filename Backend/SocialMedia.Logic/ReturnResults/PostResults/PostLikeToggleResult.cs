namespace SocialMedia.Logic.ReturnResults.PostResults;

public record PostLikeToggleResult(
    bool IsLiked, 
    string Message,
    int? Likes = 0
);