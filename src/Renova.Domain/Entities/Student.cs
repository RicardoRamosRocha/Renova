namespace Renova.Domain.Entities;

public class Student : BaseTenantEntity
{
    public Guid? PersonId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateTime BirthDate { get; set; }

    public string CPF { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public int Status { get; set; }

    public DateTime AdmissionDate { get; set; }

    public AdmissionType? AdmissionType { get; set; }

    public StudentStatus? StudentStatus { get; set; }

    public BloodType? BloodType { get; set; }

    public bool HasAllergy { get; set; }

    public string? AllergyDescription { get; set; }

    public bool UsesMedication { get; set; }

    public string? MedicationDescription { get; set; }

    public bool HasDisability { get; set; }

    public string? DisabilityDescription { get; set; }

    public string? Observation { get; set; }

    public Person? Person { get; set; }

    public ICollection<FamilyMember> FamilyMembers { get; set; } = [];

    public ICollection<Admission> Admissions { get; set; } = [];

    public MedicalRecord? MedicalRecord { get; set; }

    public string? PhotoPath { get; set; }

    public ICollection<MedicalEvolution> MedicalEvolutions { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<StudentProgress> ProgressEntries { get; set; } = [];

    public ICollection<Certificate> Certificates { get; set; } = [];

    public ICollection<Payment> Payments { get; set; } = [];

    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
