namespace Renova.API.DTOs;

public record StudentProgressResponse(
    Guid Id,
    Guid StudentId,
    Guid LessonId,
    int WatchedPercentage,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
