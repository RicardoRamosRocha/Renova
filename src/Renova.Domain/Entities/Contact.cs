namespace Renova.Domain.Entities;

public class Contact : BaseTenantEntity
{
    public Guid PersonId { get; set; }

    public ContactType ContactType { get; set; }

    public string Value { get; set; } = string.Empty;

    public string? Observation { get; set; }

    public Person Person { get; set; } = null!;
}
