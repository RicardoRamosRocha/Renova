using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateLessonRequest(
    [Required]
    [MaxLength(200)]
    string Title,

    [Required]
    string Description,

    [Required]
    [MaxLength(100)]
    string VideoProvider,

    [Required]
    [MaxLength(200)]
    string VideoExternalId,

    int DurationInMinutes,

    int Order);
