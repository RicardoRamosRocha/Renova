namespace Renova.Domain.Entities;

public class Admission : BaseTenantEntity
{
    public Guid StudentId { get; set; }

    public DateTime AdmissionDate { get; set; }

    public DateTime? ExpectedDischargeDate { get; set; }

    public DateTime? DischargeDate { get; set; }

    public string? AdmissionReason { get; set; }

    public string? DischargeReason { get; set; }

    public AdmissionStatus AdmissionStatus { get; set; } = AdmissionStatus.Active;

    public string? ReferredBy { get; set; }

    public string? ResponsibleProfessional { get; set; }

    public string? DischargeApprovedBy { get; set; }

    public string? Origin { get; set; }

    public string? DestinationAfterDischarge { get; set; }

    public string? Notes { get; set; }

    public Student Student { get; set; } = null!;
}