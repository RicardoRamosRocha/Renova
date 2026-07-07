namespace Renova.Domain.Entities;

public class Certificate
{
    // TODO Sprint 4.5: evaluate tenant scoping through Student or direct TenantId in a future migration.
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public Guid CourseId { get; set; }

    public string VerificationCode { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;

    public Course Course { get; set; } = null!;
}
