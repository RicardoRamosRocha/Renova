namespace Renova.API.DTOs;

public record CourseResponse(
    Guid Id,
    string Title,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
