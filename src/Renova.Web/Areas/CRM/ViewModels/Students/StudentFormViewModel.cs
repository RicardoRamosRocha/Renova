using System.ComponentModel.DataAnnotations;

namespace Renova.Web.Areas.CRM.ViewModels.Students;

public sealed class StudentFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    [StringLength(14)]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-28);

    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Informe um email válido.")]
    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required]
    public int Status { get; set; } = StudentStatuses.Active;

    [Required(ErrorMessage = "Informe a data de entrada.")]
    [DataType(DataType.Date)]
    public DateTime AdmissionDate { get; set; } = DateTime.Today;

    [StringLength(20)]
    public string? RG { get; set; }

    [StringLength(20)]
    public string? WhatsApp { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public static class StudentStatuses
{
    public const int Active = 1;
    public const int InTreatment = 2;
    public const int DischargePlanned = 3;
    public const int Discharged = 4;
    public const int Inactive = 5;

    public static readonly IReadOnlyDictionary<int, string> Labels = new Dictionary<int, string>
    {
        [Active] = "Ativo",
        [InTreatment] = "Em tratamento",
        [DischargePlanned] = "Alta prevista",
        [Discharged] = "Alta concluída",
        [Inactive] = "Inativo"
    };

    public static string Label(int status) => Labels.TryGetValue(status, out var label) ? label : "Ativo";
}
