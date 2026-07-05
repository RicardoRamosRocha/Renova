namespace Renova.Domain.Entities;

public abstract class AuditEntity : BaseEntity
{
    public string? CreatedByUserId { get; set; }

    public string? UpdatedByUserId { get; set; }
}
