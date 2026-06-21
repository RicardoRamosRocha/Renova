namespace Renova.Domain.Entities;

public class MedicalEvolution
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public Guid ProfessionalId { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Student Student { get; set; } = null!;

    public Professional Professional { get; set; } = null!;
}
