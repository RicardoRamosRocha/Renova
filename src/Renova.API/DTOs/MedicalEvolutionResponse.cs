namespace Renova.API.DTOs;

public record MedicalEvolutionResponse(
    Guid Id,
    Guid StudentId,
    Guid ProfessionalId,
    string Description,
    DateTime CreatedAt);
