using Renova.Web.ViewModels;

namespace Renova.Web.Areas.CRM.ViewModels.Students;

public sealed class StudentIndexViewModel
{
    public string? Search { get; set; }

    public int? Status { get; set; }

    public DateTime? AdmissionFrom { get; set; }

    public DateTime? AdmissionTo { get; set; }

    public IReadOnlyDictionary<int, string> Statuses { get; set; } = StudentStatuses.Labels;

    public int Total { get; set; }

    public int Active { get; set; }

    public int InTreatment { get; set; }

    public int DischargePlanned { get; set; }

    public int Archived { get; set; }

    public PagedResult<StudentIndexItemViewModel> Students { get; set; } = new();
}

public sealed class StudentIndexItemViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhotoUrl { get; set; }

    public DateTime AdmissionDate { get; set; }

    public int Status { get; set; }

    public string? ResponsibleProfessional { get; set; }
}
