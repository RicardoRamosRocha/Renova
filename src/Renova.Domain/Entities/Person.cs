namespace Renova.Domain.Entities;

public class Person : BaseTenantEntity
{
    public string? RegistrationNumber { get; set; }

    public string? ExternalCode { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? SocialName { get; set; }

    public DateTime? BirthDate { get; set; }

    public Gender? Gender { get; set; }

    public MaritalStatus? MaritalStatus { get; set; }

    public string? Cpf { get; set; }

    public string? Rg { get; set; }

    public string? Nationality { get; set; }

    public string? BirthPlace { get; set; }

    public string? MotherName { get; set; }

public string? FatherName { get; set; }

    public string? Occupation { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Whatsapp { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public Address? Address { get; set; }

    public Student? Student { get; set; }

    public Professional? Professional { get; set; }

    public Employee? Employee { get; set; }

    public Volunteer? Volunteer { get; set; }

    public ICollection<FamilyMember> FamilyMembers { get; set; } = [];

    public ICollection<EmergencyContact> EmergencyContacts { get; set; } = [];

    public ICollection<Contact> Contacts { get; set; } = [];

    public ICollection<Document> Documents { get; set; } = [];
}
