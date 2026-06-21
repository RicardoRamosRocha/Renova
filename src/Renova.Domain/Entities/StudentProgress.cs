namespace Renova.Domain.Entities;

public class StudentProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public Guid LessonId { get; set; }

    public int WatchedPercentage { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;

    public Lesson Lesson { get; set; } = null!;
}
