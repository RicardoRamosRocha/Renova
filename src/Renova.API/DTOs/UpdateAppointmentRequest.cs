using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdateAppointmentRequest(
    Guid? ProfessionalId,

    [Required]
    DateTime ScheduledAt,

    int Status,

    string? Notes);
