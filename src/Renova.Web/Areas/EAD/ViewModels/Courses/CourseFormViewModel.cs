using System.ComponentModel.DataAnnotations;

namespace Renova.Web.Areas.EAD.ViewModels.Courses;

public sealed class CourseFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descrição.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Category { get; set; }

    [Range(1, 500)]
    public int WorkloadHours { get; set; } = 8;

    [StringLength(200)]
    public string? Teacher { get; set; }

    public string Status { get; set; } = "Publicado";

    public bool IsActive { get; set; } = true;
}
