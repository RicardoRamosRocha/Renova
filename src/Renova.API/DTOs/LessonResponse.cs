namespace Renova.API.DTOs;

public record LessonResponse(
    Guid Id,
    Guid CourseModuleId,
    string Title,
    string Description,
    string VideoProvider,
    string VideoExternalId,
    int DurationInMinutes,
    int Order,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
