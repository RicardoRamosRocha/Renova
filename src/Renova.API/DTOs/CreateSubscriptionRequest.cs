using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateSubscriptionRequest(
    [Required]
    [MaxLength(200)]
    string PlanName,

    [Range(0.01, double.MaxValue)]
    decimal Amount,

    int Status,

    [Required]
    DateTime NextDueDate);
