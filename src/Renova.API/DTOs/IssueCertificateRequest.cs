using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record IssueCertificateRequest(
    [Required]
    Guid CourseId);
