using Renova.Domain.Entities;

namespace Renova.Web.Areas.CRM.ViewModels.Appointments;

public sealed class AppointmentDetailsViewModel
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public Guid? ProfessionalId { get; init; }
    public string? ProfessionalName { get; init; }
    public DateTime ScheduledAt { get; init; }
    public AppointmentStatus Status { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
