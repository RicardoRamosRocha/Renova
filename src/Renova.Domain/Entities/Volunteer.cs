namespace Renova.Domain.Entities;

public class Volunteer : BaseTenantEntity
{
    public Guid? PersonId { get; set; }

    public string Area { get; set; } = string.Empty;

    public string? Availability { get; set; }

    public Person? Person { get; set; }
}
