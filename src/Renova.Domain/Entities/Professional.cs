namespace Renova.Domain.Entities;

public class Professional
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public string RegistrationNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string Phone { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? PhotoPath { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<MedicalEvolution> MedicalEvolutions { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];
}
