using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateMedicalEvolutionRequest(
    [Required]
    Guid ProfessionalId,

    [Required]
    string Description);
