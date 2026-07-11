namespace Renova.Web.Areas.EAD.ViewModels.Students;

public sealed class StudentLearningDashboardViewModel
{
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public int OverallProgress { get; set; }

    public int Certificates { get; set; }

    public int CompletedLessons { get; set; }

    public string? NextActivity { get; set; }

    public IReadOnlyList<StudentOptionViewModel> Students { get; set; } = [];

    public IReadOnlyList<StudentCourseProgressViewModel> Courses { get; set; } = [];

    public IReadOnlyList<StudentTrailProgressViewModel> Trails { get; set; } = [];

    public IReadOnlyList<StudentGoalViewModel> Goals { get; set; } = [];

    public IReadOnlyList<StudentAchievementViewModel> Achievements { get; set; } = [];
}

public sealed class StudentOptionViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class StudentCourseProgressViewModel
{
    public Guid CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Trail { get; set; } = string.Empty;

    public int Progress { get; set; }

    public int CompletedLessons { get; set; }

    public int TotalLessons { get; set; }
}

public sealed class StudentTrailProgressViewModel
{
    public string Trail { get; set; } = string.Empty;

    public int Courses { get; set; }

    public int Progress { get; set; }
}

public sealed class StudentGoalViewModel
{
    public string Objective { get; set; } = string.Empty;

    public string Activity { get; set; } = string.Empty;

    public string Responsible { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Progress { get; set; }
}

public sealed class StudentAchievementViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool Achieved { get; set; }

    public string Icon { get; set; } = "ph-medal";
}
