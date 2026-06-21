using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdateFamilyMemberRequest(
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
