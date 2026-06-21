using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateCourseModuleRequest(
    [Required]
    [MaxLength(200)]
    string Title,

    [Required]
    string Description,

    int Order);
