using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateFamilyMemberRequest(
    [Required]
    [MaxLength(200)]
    string FullName,

    [Required]
    [MaxLength(100)]
    string Relationship,

    [Required]
    [MaxLength(20)]
    string Phone,

    [MaxLength(200)]
    string? Email,

    bool CanAccessPortal);
