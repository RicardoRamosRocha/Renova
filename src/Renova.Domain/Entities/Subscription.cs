namespace Renova.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public string PlanName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int Status { get; set; }

    public DateTime NextDueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
}
