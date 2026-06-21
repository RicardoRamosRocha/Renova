using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdateStudentProgressRequest(
    [Required]
    Guid LessonId,

    [Range(0, 100)]
    int WatchedPercentage);
