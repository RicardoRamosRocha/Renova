namespace Renova.Domain.Entities;

public class EmergencyContact : BaseTenantEntity
{
    public Guid PersonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public RelationshipType RelationshipType { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string? Whatsapp { get; set; }

    public string? Email { get; set; }

    public string? Notes { get; set; }

    public Person Person { get; set; } = null!;
}
