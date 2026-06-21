namespace Renova.API.DTOs;

public record UserResponse(
    string Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
