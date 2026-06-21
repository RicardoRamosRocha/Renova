using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdateProfessionalRequest(
    [Required]
    [MaxLength(200)]
    string FullName,

    [Required]
    [MaxLength(100)]
    string Specialty,

    [Required]
    [MaxLength(100)]
    string RegistrationNumber,

    [MaxLength(200)]
    string? Email,

    [Required]
    [MaxLength(20)]
    string Phone,

    bool IsActive);
