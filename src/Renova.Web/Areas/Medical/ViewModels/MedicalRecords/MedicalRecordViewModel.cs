using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Renova.Web.Areas.Medical.ViewModels.MedicalRecords;

public sealed class MedicalRecordViewModel
{
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public int Age { get; set; }

    public int CareDays { get; set; }

    public string? ResponsibleProfessional { get; set; }

    public string? Anamnesis { get; set; }

    public string? ClinicalNotes { get; set; }

    public IReadOnlyList<MedicalRecordStudentOptionViewModel> Students { get; set; } = [];

    public IReadOnlyList<MedicalEvolutionItemViewModel> Evolutions { get; set; } = [];

    public IReadOnlyList<MedicalTimelineItemViewModel> Timeline { get; set; } = [];

    public IReadOnlyList<MedicalDocumentItemViewModel> Documents { get; set; } = [];

    public IReadOnlyList<TherapeuticPlanItemViewModel> TherapeuticPlan { get; set; } = [];

    public IReadOnlyList<MedicationPlaceholderViewModel> Medications { get; set; } = [];

    public IReadOnlyList<SelectListItem> Professionals { get; set; } = [];

    public MedicalEvolutionFormViewModel EvolutionForm { get; set; } = new();

    public MedicalDocumentUploadViewModel DocumentUpload { get; set; } = new();
}

public sealed class MedicalRecordStudentOptionViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class MedicalEvolutionItemViewModel
{
    public DateTime CreatedAt { get; set; }

    public string ProfessionalName { get; set; } = string.Empty;

    public string EvolutionType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Signature { get; set; } = string.Empty;
}

public sealed class MedicalTimelineItemViewModel
{
    public string Icon { get; set; } = "ph-circle";

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime OccurredAt { get; set; }

    public string Category { get; set; } = string.Empty;

    public string? Responsible { get; set; }
}

public sealed class MedicalDocumentItemViewModel
{
    public string Title { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public DateTime UploadDate { get; set; }

    public string? UploadedBy { get; set; }
}

public sealed class TherapeuticPlanItemViewModel
{
    public string Objective { get; set; } = string.Empty;

    public string Goal { get; set; } = string.Empty;

    public string Intervention { get; set; } = string.Empty;

    public string Periodicity { get; set; } = string.Empty;

    public string Responsible { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Conclusion { get; set; }
}

public sealed class MedicationPlaceholderViewModel
{
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public sealed class MedicalEvolutionFormViewModel
{
    [Required]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Selecione o profissional.")]
    public Guid ProfessionalId { get; set; }

    [Required(ErrorMessage = "Descreva a evolução.")]
    [StringLength(4000)]
    public string Description { get; set; } = string.Empty;
}

public sealed class MedicalDocumentUploadViewModel
{
    [Required]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Informe o título do documento.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione um arquivo.")]
    public IFormFile? File { get; set; }
}
