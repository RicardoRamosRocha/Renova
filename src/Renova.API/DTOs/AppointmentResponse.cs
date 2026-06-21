namespace Renova.API.DTOs;

public record AppointmentResponse(
    Guid Id,
    Guid StudentId,
    Guid? ProfessionalId,
    DateTime ScheduledAt,
    int Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
