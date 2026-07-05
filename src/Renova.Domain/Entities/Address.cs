namespace Renova.Domain.Entities;

public class Address : BaseTenantEntity
{
    public Guid PersonId { get; set; }

    public string Street { get; set; } = string.Empty;

    public string? Number { get; set; }

    public string? Neighborhood { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string? Complement { get; set; }

    public Person Person { get; set; } = null!;
}
