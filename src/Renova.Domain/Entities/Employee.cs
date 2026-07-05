namespace Renova.Domain.Entities;

public class Employee : BaseTenantEntity
{
    public Guid? PersonId { get; set; }

    public string Department { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public DateTime AdmissionDate { get; set; }

    public Person? Person { get; set; }
}
