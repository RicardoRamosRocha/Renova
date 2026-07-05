namespace Renova.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string LegalName { get; set; } = string.Empty;

    public string Cnpj { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public bool IsActive { get; set; } = true;

    public TenantSettings? Settings { get; set; }

    public ICollection<TenantSubscription> Subscriptions { get; set; } = [];
}
