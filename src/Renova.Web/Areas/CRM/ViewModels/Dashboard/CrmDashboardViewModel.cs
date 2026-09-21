using Renova.Domain.Entities;

namespace Renova.Web.Areas.CRM.ViewModels.Dashboard;

public sealed class CrmDashboardViewModel
{
    public int ActiveStudents { get; set; }
    public int ActiveAdmissions { get; set; }
    public int ActiveFamilies { get; set; }
    public int UpcomingAppointments { get; set; }
    public IReadOnlyList<CrmDashboardAppointmentViewModel> UpcomingAppointmentItems { get; set; } = [];
    public IReadOnlyList<CrmDashboardAdmissionViewModel> RecentAdmissions { get; set; } = [];
    public IReadOnlyList<CrmDashboardStudentViewModel> RecentStudents { get; set; } = [];
    public IReadOnlyList<CrmDashboardFamilyViewModel> RecentFamilies { get; set; } = [];
}

public sealed class CrmDashboardAppointmentViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public string? ProfessionalName { get; set; }
    public AppointmentStatus Status { get; set; }
}

public sealed class CrmDashboardAdmissionViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public AdmissionStatus Status { get; set; }
    public DateTime? DischargeDate { get; set; }
}

public sealed class CrmDashboardStudentViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ActiveAdmissionId { get; set; }
}

public sealed class CrmDashboardFamilyViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
