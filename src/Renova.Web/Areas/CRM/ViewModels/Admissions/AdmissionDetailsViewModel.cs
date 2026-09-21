using Renova.Domain.Entities;

namespace Renova.Web.Areas.CRM.ViewModels.Admissions;

public sealed class AdmissionDetailsViewModel
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public DateTime? ExpectedDischargeDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? AdmissionReason { get; set; }
    public string? DischargeReason { get; set; }
    public AdmissionStatus AdmissionStatus { get; set; }
    public string? ReferredBy { get; set; }
    public string? ResponsibleProfessional { get; set; }
    public string? DischargeApprovedBy { get; set; }
    public string? Origin { get; set; }
    public string? DestinationAfterDischarge { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
