using System.ComponentModel.DataAnnotations;
using Renova.Web.Areas.EAD.ViewModels.Students;

namespace Renova.Web.Areas.EAD.ViewModels.Courses;

public sealed class CourseContentViewModel
{
    public Guid CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public IReadOnlyList<CourseModuleDetailsViewModel> Modules { get; set; } = [];
}

public sealed class CourseModuleFormViewModel
{
    public Guid? Id { get; set; }

    public Guid CourseId { get; set; }

    [Required(ErrorMessage = "Informe o titulo do modulo.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descricao do modulo.")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 999, ErrorMessage = "Informe uma ordem entre 1 e 999.")]
    public int Order { get; set; } = 1;
}

public sealed class LessonFormViewModel
{
    public Guid? Id { get; set; }

    public Guid CourseId { get; set; }

    public Guid ModuleId { get; set; }

    [Required(ErrorMessage = "Informe o titulo da aula.")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descricao da aula.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o provedor.")]
    [StringLength(100)]
    public string VideoProvider { get; set; } = "YouTube";

    [StringLength(200)]
    public string VideoExternalId { get; set; } = string.Empty;

    [Range(1, 600, ErrorMessage = "Informe uma duracao entre 1 e 600 minutos.")]
    public int DurationInMinutes { get; set; } = 15;

    [Range(1, 999, ErrorMessage = "Informe uma ordem entre 1 e 999.")]
    public int Order { get; set; } = 1;
}

public sealed class LessonPlayerViewModel
{
    public Guid CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public Guid LessonId { get; set; }

    public string LessonTitle { get; set; } = string.Empty;

    public string LessonDescription { get; set; } = string.Empty;

    public string VideoProvider { get; set; } = string.Empty;

    public string VideoExternalId { get; set; } = string.Empty;

    public int DurationInMinutes { get; set; }

    public Guid? StudentId { get; set; }

    public string? StudentName { get; set; }

    public int Progress { get; set; }

    public bool IsCompleted { get; set; }

    public Guid? PreviousLessonId { get; set; }

    public Guid? NextLessonId { get; set; }

    public IReadOnlyList<StudentOptionViewModel> Students { get; set; } = [];

    public IReadOnlyList<CourseModuleDetailsViewModel> Modules { get; set; } = [];
}
