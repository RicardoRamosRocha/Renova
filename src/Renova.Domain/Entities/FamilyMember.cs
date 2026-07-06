using System.ComponentModel.DataAnnotations.Schema;

namespace Renova.Domain.Entities;

public class FamilyMember : BaseTenantEntity
{
    public Guid StudentId { get; set; }

    public Guid? PersonId { get; set; }

    public Person? Person { get; set; }

    [NotMapped]
    public string DisplayName => Prefer(Person?.FullName, FullName);

    [NotMapped]
    public string? DisplayEmail => PreferNullable(Person?.Email, Email);

    [NotMapped]
    public string DisplayPhone => Prefer(Person?.Phone, Phone);

    [NotMapped]
    public string? DisplayPhotoUrl => PreferNullable(Person?.PhotoUrl, PhotoPath);

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.FullName for new flows.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// LEGACY: relationship text kept for compatibility. Prefer RelationshipType when possible.
    /// </summary>
    public string Relationship { get; set; } = string.Empty;

    public RelationshipType? RelationshipType { get; set; }

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Phone, Person.Mobile, or contacts for new flows.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// LEGACY: personal data kept for compatibility. Prefer Person.Email for new flows.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// LEGACY: personal profile photo kept for compatibility. Prefer Person.PhotoUrl for new flows.
    /// </summary>
    public string? PhotoPath { get; set; }

    public bool IsResponsible { get; set; }

    public bool CanAccessPortal { get; set; }

    public Student Student { get; set; } = null!;

    public void SyncPersonFromLegacyFields(DateTime timestamp, bool markPersonAsUpdated = false)
    {
        Person ??= new Person
        {
            CreatedAt = timestamp
        };

        PersonId = Person.Id;
        Person.TenantId = TenantId;
        Person.FullName = FullName;
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
