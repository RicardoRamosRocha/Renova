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

    public string CourseCategory { get; set; } = string.Empty;

    public string CourseLevel { get; set; } = string.Empty;

    public string CourseTeacher { get; set; } = string.Empty;

    public int CourseWorkloadHours { get; set; }

    public Guid LessonId { get; set; }

    public string LessonTitle { get; set; } = string.Empty;

    public string LessonDescription { get; set; } = string.Empty;

    public string ModuleTitle { get; set; } = string.Empty;

    public string VideoProvider { get; set; } = string.Empty;

    public string VideoExternalId { get; set; } = string.Empty;

    public int DurationInMinutes { get; set; }

    public Guid? StudentId { get; set; }

    public string? StudentName { get; set; }

    public int Progress { get; set; }

    public bool IsCompleted { get; set; }

    public int CourseProgress { get; set; }

    public Guid? PreviousLessonId { get; set; }

    public Guid? NextLessonId { get; set; }

    public IReadOnlySet<Guid> CompletedLessonIds { get; set; } = new HashSet<Guid>();

    public IReadOnlyList<StudentOptionViewModel> Students { get; set; } = [];

    public IReadOnlyList<CourseModuleDetailsViewModel> Modules { get; set; } = [];

    public IReadOnlyList<string> Objectives { get; set; } = [];

    public IReadOnlyList<LessonMaterialViewModel> Materials { get; set; } = [];

    public IReadOnlyList<LessonDownloadViewModel> Downloads { get; set; } = [];

    public LessonQuizViewModel Quiz { get; set; } = new();

    public IReadOnlyList<LessonDiscussionViewModel> Discussions { get; set; } = [];

    public IReadOnlyList<string> Notes { get; set; } = [];

    public LessonTeacherViewModel Teacher { get; set; } = new();
}

public sealed class LessonMaterialViewModel
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    public string Icon { get; set; } = "ph-file";
}

public sealed class LessonDownloadViewModel
{
    public string Name { get; set; } = string.Empty;

    public string Format { get; set; } = string.Empty;

    public string Size { get; set; } = string.Empty;

    public string Icon { get; set; } = "ph-download-simple";
}

public sealed class LessonQuizViewModel
{
    public string Title { get; set; } = string.Empty;

    public int PassingScore { get; set; } = 70;

    public IReadOnlyList<LessonQuizQuestionViewModel> Questions { get; set; } = [];
}

public sealed class LessonQuizQuestionViewModel
{
    public string Text { get; set; } = string.Empty;

    public IReadOnlyList<LessonQuizAnswerViewModel> Answers { get; set; } = [];
}

public sealed class LessonQuizAnswerViewModel
{
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

public sealed class LessonDiscussionViewModel
{
    public string Author { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public int Likes { get; set; }
}

public sealed class LessonTeacherViewModel
{
    public string Name { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public int Courses { get; set; }
}
