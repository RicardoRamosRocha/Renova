namespace Renova.Domain.Entities;

public class MedicalRecord
{
    // TODO Sprint 4: evaluate BaseTenantEntity adoption after a migration plan adds TenantId and IsDeleted.
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public string Anamnesis { get; set; } = string.Empty;

    public string ClinicalNotes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
}
