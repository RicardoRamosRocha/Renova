namespace Renova.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }

    public decimal Amount { get; set; }

    public int Status { get; set; }

    public int PaymentMethod { get; set; }

    public string? ExternalPaymentId { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
}
