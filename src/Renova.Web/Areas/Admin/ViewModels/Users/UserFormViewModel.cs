using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Renova.Web.Areas.Admin.ViewModels.Users;

public sealed class UserFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o email.")]
    [EmailAddress(ErrorMessage = "Informe um email válido.")]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    [StringLength(20)]
    public string? Phone { get; set; }

    public string? Role { get; set; }

    [DataType(DataType.Password)]
    public string? TemporaryPassword { get; set; }

    public IFormFile? Photo { get; set; }

    public string? PhotoPath { get; set; }

    public bool RemovePhoto { get; set; }
}
