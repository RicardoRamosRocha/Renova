using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateAppointmentRequest(
    Guid? ProfessionalId,

    [Required]
    DateTime ScheduledAt,

    int Status,

    string? Notes);
