using System.ComponentModel.DataAnnotations;

namespace Renova.Web.Areas.Medical.ViewModels.Professionals;

public sealed class ProfessionalFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(14)]
    public string? CPF { get; set; }

    [Required(ErrorMessage = "Informe o registro profissional.")]
    [StringLength(100)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a especialidade.")]
    [StringLength(100)]
    public string Specialty { get; set; } = "Psicólogo";

    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(20)]
    public string? WhatsApp { get; set; }

    [EmailAddress(ErrorMessage = "Informe um email válido.")]
    [StringLength(200)]
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(1000)]
    public string? Notes { get; set; }
}
