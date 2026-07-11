namespace Renova.Web.Areas.CRM.ViewModels.Students;

public sealed class StudentDetailsViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? PhotoUrl { get; set; }

    public DateTime BirthDate { get; set; }

    public DateTime AdmissionDate { get; set; }

    public int Status { get; set; }

    public string? TenantName { get; set; }

    public string? ResponsibleProfessional { get; set; }

    public int Age { get; set; }

    public int TreatmentDays { get; set; }

    public IReadOnlyList<StudentAdmissionSummaryViewModel> Admissions { get; set; } = [];

    public IReadOnlyList<StudentFamilySummaryViewModel> FamilyMembers { get; set; } = [];

    public IReadOnlyList<StudentAppointmentSummaryViewModel> Appointments { get; set; } = [];

    public IReadOnlyList<StudentMedicalEvolutionSummaryViewModel> MedicalEvolutions { get; set; } = [];

    public IReadOnlyList<TimelineItemViewModel> Timeline { get; set; } = [];
}

public sealed class StudentAdmissionSummaryViewModel
{
    public Guid Id { get; set; }

    public DateTime AdmissionDate { get; set; }

    public DateTime? ExpectedDischargeDate { get; set; }

    public DateTime? DischargeDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? ResponsibleProfessional { get; set; }
}

public sealed class StudentFamilySummaryViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Relationship { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsResponsible { get; set; }

    public bool CanAccessPortal { get; set; }
}

public sealed class StudentAppointmentSummaryViewModel
{
    public DateTime ScheduledAt { get; set; }

    public string ProfessionalName { get; set; } = "Profissional a definir";
}

public sealed class StudentMedicalEvolutionSummaryViewModel
{
    public DateTime CreatedAt { get; set; }

    public string ProfessionalName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class TimelineItemViewModel
{
    public string Icon { get; set; } = "ph-circle";

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime OccurredAt { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? Responsible { get; set; }
}
