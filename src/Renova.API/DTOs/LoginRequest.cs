using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record LoginRequest(
    [Required]
    [EmailAddress]
    string Email,

    [Required]
    string Password);
