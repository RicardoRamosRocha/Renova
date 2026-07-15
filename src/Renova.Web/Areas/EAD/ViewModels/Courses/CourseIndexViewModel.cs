using Renova.Web.ViewModels;

namespace Renova.Web.Areas.EAD.ViewModels.Courses;

public sealed class CourseIndexViewModel
{
    public string? Search { get; set; }

    public bool? Active { get; set; }

    public string? Trail { get; set; }

    public int TotalCourses { get; set; }

    public int ActiveCourses { get; set; }

    public int DraftCourses { get; set; }

    public int StudyingStudents { get; set; }

    public int CompletionsThisMonth { get; set; }

    public int Certificates { get; set; }

    public int AverageProgress { get; set; }

    public int CompletionRate { get; set; }

    public IReadOnlyList<string> Trails { get; set; } = [];

    public PagedResult<CourseIndexItemViewModel> Courses { get; set; } = new();
}

public sealed class CourseIndexItemViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Trail { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Teacher { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public int WorkloadHours { get; set; }

    public int DurationInMinutes { get; set; }

    public int Modules { get; set; }

    public int Lessons { get; set; }

    public int Students { get; set; }

    public int Certificates { get; set; }

    public int AverageProgress { get; set; }

    public int CompletionRate { get; set; }

    public string Icon { get; set; } = "ph-book-open";
}

public sealed class CourseDetailsViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Trail { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Teacher { get; set; } = string.Empty;

    public int WorkloadHours { get; set; }

    public int Lessons { get; set; }

    public int Students { get; set; }

    public int Certificates { get; set; }

    public int AverageProgress { get; set; }

    public int CompletionRate { get; set; }

    public IReadOnlyList<CourseModuleDetailsViewModel> Modules { get; set; } = [];
}

public sealed class CourseModuleDetailsViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public IReadOnlyList<CourseLessonDetailsViewModel> Lessons { get; set; } = [];

    public int AverageProgress => Lessons.Count == 0 ? 0 : (int)Math.Round(Lessons.Average(item => item.AverageProgress));
}

public sealed class CourseLessonDetailsViewModel
{
    public Guid Id { get; set; }

    public Guid ModuleId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string VideoProvider { get; set; } = string.Empty;

    public string VideoExternalId { get; set; } = string.Empty;

    public int DurationInMinutes { get; set; }

    public int Order { get; set; }

    public int Students { get; set; }

    public int AverageProgress { get; set; }

    public int CompletionRate { get; set; }
}
