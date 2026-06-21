using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateUserRequest(
    [Required]
    [MaxLength(200)]
    string FullName,

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    string Email,

    [Required]
    [MaxLength(20)]
    string PhoneNumber,

    [Required]
    string Password,

    [Required]
    string Role);
