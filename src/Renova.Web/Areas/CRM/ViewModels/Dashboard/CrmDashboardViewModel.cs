namespace Renova.Web.Areas.CRM.ViewModels.Dashboard;

public sealed class CrmDashboardViewModel
{
    public int ActiveStudents { get; set; }

    public int AdmissionsThisMonth { get; set; }

    public int ExpectedDischarges { get; set; }

    public int CompletedDischarges { get; set; }

    public int Transfers { get; set; }

    public int BirthdaysThisMonth { get; set; }

    public IReadOnlyList<CrmDashboardStudentViewModel> LatestStudents { get; set; } = [];

    public IReadOnlyList<CrmDashboardAdmissionViewModel> UpcomingDischarges { get; set; } = [];

    public IReadOnlyList<CrmDashboardAppointmentViewModel> TodayAppointments { get; set; } = [];
}

public sealed class CrmDashboardStudentViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public DateTime CreatedAt { get; set; }
}

public sealed class CrmDashboardAdmissionViewModel
{
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public DateTime ExpectedDischargeDate { get; set; }

    public string? ResponsibleProfessional { get; set; }
}

public sealed class CrmDashboardAppointmentViewModel
{
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    public string? ProfessionalName { get; set; }
}
