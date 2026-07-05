namespace Renova.Domain.Entities;

public abstract class BaseTenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
