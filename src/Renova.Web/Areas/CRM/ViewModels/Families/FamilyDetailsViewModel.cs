using Renova.Domain.Entities;

namespace Renova.Web.Areas.CRM.ViewModels.Families;

public sealed class FamilyDetailsViewModel
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Relationship { get; init; } = string.Empty;
    public RelationshipType? RelationshipType { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhotoUrl { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public bool IsResponsible { get; init; }
    public bool CanAccessPortal { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
