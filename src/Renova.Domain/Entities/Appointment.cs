namespace Renova.Domain.Entities;

public class Appointment
{
    // TODO Sprint 4.5: evaluate BaseTenantEntity adoption after a migration plan adds TenantId and IsDeleted.
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public Guid? ProfessionalId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;

    public Professional? Professional { get; set; }
}
