using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdateCourseRequest(
    [Required]
    [MaxLength(200)]
    string Title,

    [Required]
    string Description,

    bool IsActive);
