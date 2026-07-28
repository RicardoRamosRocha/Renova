namespace Renova.Web.Areas.EAD.ViewModels.Dashboard;

public sealed class EadDashboardViewModel
{
    public int ActiveCourses { get; set; }

    public int TotalCourses { get; set; }

    public int AvailableLessons { get; set; }

    public int StudyingStudents { get; set; }

    public int CompletedLessons { get; set; }

    public int LearningHours { get; set; }

    public int Certificates { get; set; }

    public int AverageEngagement { get; set; }

    public int CompletionRate { get; set; }

    public int StudentsNeedingAttention { get; set; }

    public IReadOnlyList<CoursePerformanceViewModel> TopCourses { get; set; } = [];

    public IReadOnlyList<TrailSummaryViewModel> Trails { get; set; } = [];

    public IReadOnlyList<RecentLearningActivityViewModel> RecentActivities { get; set; } = [];
}

public sealed class CoursePerformanceViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Trail { get; set; } = string.Empty;

    public int Students { get; set; }

    public int AverageProgress { get; set; }

    public int CompletionRate { get; set; }
}

public sealed class TrailSummaryViewModel
{
    public string Name { get; set; } = string.Empty;

    public int Courses { get; set; }

    public int Students { get; set; }

    public int AverageProgress { get; set; }

    public string Icon { get; set; } = "ph-path";
}

public sealed class RecentLearningActivityViewModel
{
    public string StudentName { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public string LessonTitle { get; set; } = string.Empty;

    public int Progress { get; set; }

    public DateTime OccurredAt { get; set; }
}
