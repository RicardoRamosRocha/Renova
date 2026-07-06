namespace Renova.Domain.Entities;

public class Professional : BaseTenantEntity
{
    public Guid? PersonId { get; set; }

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.FullName for new flows.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    public ProfessionalType? ProfessionalType { get; set; }

    public string Specialty { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Email for new flows.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Phone, Person.Mobile, or contacts for new flows.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// LEGACY: personal profile photo kept for compatibility. Prefer Person.PhotoUrl for new flows.
    /// </summary>
    public string? PhotoPath { get; set; }

    public Person? Person { get; set; }

    public ICollection<MedicalEvolution> MedicalEvolutions { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];
}
