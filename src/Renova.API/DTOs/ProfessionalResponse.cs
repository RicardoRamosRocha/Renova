namespace Renova.API.DTOs;

public record ProfessionalResponse(
    Guid Id,
    string FullName,
    string Specialty,
    string RegistrationNumber,
    string? Email,
    string Phone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
