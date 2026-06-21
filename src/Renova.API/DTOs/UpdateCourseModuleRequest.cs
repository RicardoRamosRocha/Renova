using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdateCourseModuleRequest(
    [Required]
    [MaxLength(200)]
    string Title,

    [Required]
    string Description,

    int Order);
