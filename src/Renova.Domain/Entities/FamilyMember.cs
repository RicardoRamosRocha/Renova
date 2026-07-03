namespace Renova.Domain.Entities;

public class FamilyMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Relationship { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhotoPath { get; set; }

    public bool CanAccessPortal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
}
