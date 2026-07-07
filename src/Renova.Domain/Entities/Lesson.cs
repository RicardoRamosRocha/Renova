namespace Renova.Domain.Entities;

public class Lesson
{
    // TODO Sprint 4.5: follow Course tenancy decision; changing inheritance requires a migration plan.
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CourseModuleId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string VideoProvider { get; set; } = string.Empty;

    public string VideoExternalId { get; set; } = string.Empty;

    public int DurationInMinutes { get; set; }

    public int Order { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public CourseModule CourseModule { get; set; } = null!;

    public ICollection<StudentProgress> ProgressEntries { get; set; } = [];
}
