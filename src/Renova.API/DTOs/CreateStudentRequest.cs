using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateStudentRequest(
    [Required]
    [MaxLength(200)]
    string FullName,

    [Required]
    DateTime BirthDate,

    [Required]
    [MaxLength(14)]
    string CPF,

    [Required]
    [MaxLength(20)]
    string Phone,

    [MaxLength(200)]
    string? Email,

    [MaxLength(500)]
    string? Address,

    int Status,

    [Required]
    DateTime AdmissionDate);
