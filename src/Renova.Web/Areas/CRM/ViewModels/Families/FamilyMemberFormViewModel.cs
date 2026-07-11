using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Renova.Domain.Entities;

namespace Renova.Web.Areas.CRM.ViewModels.Families;

public sealed class FamilyMemberFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome do familiar.")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o parentesco.")]
    [StringLength(100)]
    public string Relationship { get; set; } = string.Empty;

    public RelationshipType? RelationshipType { get; set; }

    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Informe um email válido.")]
    [StringLength(200)]
    public string? Email { get; set; }

    public bool IsResponsible { get; set; }

    public bool CanAccessPortal { get; set; }

    public IFormFile? Photo { get; set; }

    public string? PhotoPath { get; set; }

    public bool RemovePhoto { get; set; }
}
