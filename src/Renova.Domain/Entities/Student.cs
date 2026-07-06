using System.ComponentModel.DataAnnotations.Schema;

namespace Renova.Domain.Entities;

public class Student : BaseTenantEntity
{
    public Guid? PersonId { get; set; }

    public Person? Person { get; set; }

    [NotMapped]
    public string DisplayName => Prefer(Person?.FullName, FullName);

    [NotMapped]
    public string DisplayCpf => Prefer(Person?.Cpf, CPF);

    [NotMapped]
    public string? DisplayEmail => PreferNullable(Person?.Email, Email);

    [NotMapped]
    public string DisplayPhone => Prefer(Person?.Phone, Phone);

    [NotMapped]
    public DateTime DisplayBirthDate => Person?.BirthDate ?? BirthDate;

    [NotMapped]
    public string? DisplayPhotoUrl => PreferNullable(Person?.PhotoUrl, PhotoPath);

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.FullName for new flows.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.BirthDate for new flows.
    /// </summary>
    public DateTime BirthDate { get; set; }

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Cpf for new flows.
    /// </summary>
    public string CPF { get; set; } = string.Empty;

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Phone, Person.Mobile, or contacts for new flows.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Email for new flows.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Address for new flows.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// LEGACY: numeric status kept for compatibility. Prefer StudentStatus for new flows.
    /// </summary>
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

    public ICollection<FamilyMember> FamilyMembers { get; set; } = [];

    public ICollection<Admission> Admissions { get; set; } = [];

    public MedicalRecord? MedicalRecord { get; set; }

    /// <summary>
    /// LEGACY: personal profile photo kept for compatibility. Prefer Person.PhotoUrl for new flows.
    /// </summary>
    public string? PhotoPath { get; set; }

    public ICollection<MedicalEvolution> MedicalEvolutions { get; set; } = [];

    public ICollection<Appointment> Appointments { get; set; } = [];

    public ICollection<StudentProgress> ProgressEntries { get; set; } = [];

    public ICollection<Certificate> Certificates { get; set; } = [];

    public ICollection<Payment> Payments { get; set; } = [];

    public ICollection<Subscription> Subscriptions { get; set; } = [];

    public void SyncPersonFromLegacyFields(DateTime timestamp, bool markPersonAsUpdated = false)
    {
        Person ??= new Person
        {
            CreatedAt = timestamp
        };

        PersonId = Person.Id;
        Person.TenantId = TenantId;
        Person.FullName = FullName;
        Person.BirthDate = BirthDate;
        Person.Cpf = CPF;
        Person.Email = Email;
        Person.Phone = Phone;
        Person.PhotoUrl = PhotoPath;
        Person.IsActive = !IsDeleted;

        if (markPersonAsUpdated)
        {
            Person.UpdatedAt = timestamp;
        }
    }

    private static string Prefer(string? primary, string fallback)
    {
        return string.IsNullOrWhiteSpace(primary) ? fallback : primary;
    }

    private static string? PreferNullable(string? primary, string? fallback)
    {
        return string.IsNullOrWhiteSpace(primary) ? fallback : primary;
    }
}
