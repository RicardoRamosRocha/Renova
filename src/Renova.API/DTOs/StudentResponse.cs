namespace Renova.API.DTOs;

public record StudentResponse(
    Guid Id,
    string FullName,
    DateTime BirthDate,
    string CPF,
    string Phone,
    string? Email,
    string? Address,
    int Status,
    DateTime AdmissionDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);