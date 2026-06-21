namespace Renova.API.DTOs;

public record CourseModuleResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    string Description,
    int Order,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
