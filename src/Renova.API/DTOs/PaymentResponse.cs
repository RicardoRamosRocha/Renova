namespace Renova.API.DTOs;

public record PaymentResponse(
    Guid Id,
    Guid StudentId,
    decimal Amount,
    int Status,
    int PaymentMethod,
    string? ExternalPaymentId,
    DateTime? PaidAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
