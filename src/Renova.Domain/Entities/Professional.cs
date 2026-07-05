namespace Renova.Domain.Entities;

public class Professional : BaseTenantEntity
{
    public Guid? PersonId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public ProfessionalType? ProfessionalType { get; set; }

    public string Specialty { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Phone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string? PhotoPath { get; set; }

    public Person? Person { get; set; }

    public ICollection<MedicalEvolution> MedicalEvolutions { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];
}
