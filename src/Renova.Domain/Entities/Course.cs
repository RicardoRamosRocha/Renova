namespace Renova.Domain.Entities;

public class Course
{
    // TODO Sprint 4.5: decide whether EAD catalog remains global or becomes tenant-scoped before adding migrations.
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<CourseModule> Modules { get; set; } = [];

    public ICollection<Certificate> Certificates { get; set; } = [];
}
