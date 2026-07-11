using System.ComponentModel.DataAnnotations;
using Renova.Domain.Entities;

namespace Renova.Web.Areas.CRM.ViewModels.Admissions;

public sealed class AdmissionFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de entrada.")]
    [DataType(DataType.Date)]
    public DateTime AdmissionDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime? ExpectedDischargeDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DischargeDate { get; set; }

    [StringLength(1000)]
    public string? AdmissionReason { get; set; }

    [StringLength(1000)]
    public string? DischargeReason { get; set; }

    [Required]
    public AdmissionStatus AdmissionStatus { get; set; } = AdmissionStatus.Active;

    [StringLength(200)]
    public string? ReferredBy { get; set; }

    [StringLength(200)]
    public string? ResponsibleProfessional { get; set; }

    [StringLength(200)]
    public string? DischargeApprovedBy { get; set; }

    [StringLength(200)]
    public string? Origin { get; set; }

    [StringLength(200)]
    public string? DestinationAfterDischarge { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}
