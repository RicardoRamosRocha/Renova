namespace Renova.Domain.Entities;

public class FamilyMember : BaseTenantEntity
{
    public Guid StudentId { get; set; }

    public Guid? PersonId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Relationship { get; set; } = string.Empty;

    public RelationshipType? RelationshipType { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhotoPath { get; set; }

    public bool IsResponsible { get; set; }

    public bool CanAccessPortal { get; set; }

    public Student Student { get; set; } = null!;

    public Person? Person { get; set; }
}
