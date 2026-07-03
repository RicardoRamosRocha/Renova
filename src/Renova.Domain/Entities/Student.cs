namespace Renova.Domain.Entities;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public string CPF { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public int Status { get; set; }

    public DateTime AdmissionDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<FamilyMember> FamilyMembers { get; set; } = [];

    public MedicalRecord? MedicalRecord { get; set; }

    public string? PhotoPath { get; set; }

    public ICollection<MedicalEvolution> MedicalEvolutions { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<StudentProgress> ProgressEntries { get; set; } = [];

    public ICollection<Certificate> Certificates { get; set; } = [];

    public ICollection<Payment> Payments { get; set; } = [];

    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
