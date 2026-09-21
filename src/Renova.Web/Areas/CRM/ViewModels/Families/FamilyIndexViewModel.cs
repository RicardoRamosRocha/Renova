using Renova.Web.ViewModels;

namespace Renova.Web.Areas.CRM.ViewModels.Families;

public sealed class FamilyIndexViewModel
{
    public string? Search { get; init; }
    public Guid? StudentId { get; init; }
    public bool IncludeArchived { get; init; }
    public int Total { get; init; }
    public int Active { get; init; }
    public int Archived { get; init; }
    public PagedResult<FamilyIndexItemViewModel> Families { get; init; } = new();
}

public sealed class FamilyIndexItemViewModel
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string Relationship { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhotoUrl { get; init; }
    public bool IsResponsible { get; init; }
    public bool CanAccessPortal { get; init; }
    public bool IsArchived { get; init; }
}
