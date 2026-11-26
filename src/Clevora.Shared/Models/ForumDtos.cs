namespace Clevora.Shared.Models;

public record ForumThreadDto(
    int Id,
    string Category,
    string Title,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    string Tags,
    int Upvotes,
    int CommentCount,
    DateTime CreatedAt,
    bool HasUpvoted = false
);

public record ForumCommentDto(
    int Id,
    int ThreadId,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    DateTime CreatedAt
);

public record CreateForumThreadDto(
    string Category,
    string Title,
    string Content,
    string Tags,
    string AuthorName
);

public record CreateForumCommentDto(
    int ThreadId,
    string AuthorName,
    string Content
);
