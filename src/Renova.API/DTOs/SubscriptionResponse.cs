namespace Renova.API.DTOs;

public record SubscriptionResponse(
    Guid Id,
    Guid StudentId,
    string PlanName,
    decimal Amount,
    int Status,
    DateTime NextDueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
