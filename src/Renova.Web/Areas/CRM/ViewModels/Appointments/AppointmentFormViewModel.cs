using System.ComponentModel.DataAnnotations;
using Renova.Domain.Entities;

namespace Renova.Web.Areas.CRM.ViewModels.Appointments;

public sealed class AppointmentFormViewModel
{
    public Guid Id { get; set; }
    [Required] public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid? ProfessionalId { get; set; }
    [Required] public DateTime ScheduledAt { get; set; }
    [Required] public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    [StringLength(2000)] public string? Notes { get; set; }
    public IReadOnlyList<AppointmentOptionViewModel> Students { get; set; } = [];
    public IReadOnlyList<AppointmentOptionViewModel> Professionals { get; set; } = [];
}

public sealed class AppointmentOptionViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
