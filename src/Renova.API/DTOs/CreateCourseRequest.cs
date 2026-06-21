using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateCourseRequest(
    [Required]
    [MaxLength(200)]
    string Title,

    [Required]
    string Description,

    bool IsActive);
