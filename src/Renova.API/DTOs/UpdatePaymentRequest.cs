using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdatePaymentRequest(
    [Range(0.01, double.MaxValue)]
    decimal Amount,

    int Status,

    int PaymentMethod,

    [MaxLength(200)]
    string? ExternalPaymentId,

    DateTime? PaidAt);
