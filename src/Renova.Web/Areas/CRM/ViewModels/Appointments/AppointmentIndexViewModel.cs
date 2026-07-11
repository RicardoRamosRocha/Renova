namespace Renova.Web.Areas.CRM.ViewModels.Appointments;

public sealed class AppointmentIndexViewModel
{
    public DateTime Date { get; set; }

    public int Total { get; set; }

    public int Scheduled { get; set; }

    public int Completed { get; set; }

    public int Cancelled { get; set; }

    public IReadOnlyList<AppointmentIndexItemViewModel> Appointments { get; set; } = [];
}

public sealed class AppointmentIndexItemViewModel
{
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? StudentPhotoUrl { get; set; }

    public string? ProfessionalName { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int Status { get; set; }

    public string? Notes { get; set; }
}
