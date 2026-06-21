namespace Renova.API.DTOs;

public record FamilyMemberResponse(
    Guid Id,
    Guid StudentId,
    string FullName,
    string Relationship,
    string Phone,
    string? Email,
    bool CanAccessPortal,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
