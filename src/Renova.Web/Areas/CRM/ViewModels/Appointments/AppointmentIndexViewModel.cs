namespace Renova.Web.Areas.CRM.ViewModels.Appointments;

public sealed class AppointmentIndexViewModel
{
    public string? Search { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? Status { get; set; }
    public Guid? ProfessionalId { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
    public IReadOnlyList<AppointmentOptionViewModel> Professionals { get; set; } = [];

    public int Total { get; set; }

    public int Scheduled { get; set; }

    public int Completed { get; set; }

    public int Cancelled { get; set; }

    public IReadOnlyList<AppointmentIndexItemViewModel> Appointments { get; set; } = [];
}

public sealed class AppointmentIndexItemViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? StudentPhotoUrl { get; set; }

    public string? ProfessionalName { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int Status { get; set; }

    public string? Notes { get; set; }

    public string StatusLabel { get; set; } = string.Empty;
}
