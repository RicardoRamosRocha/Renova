namespace Renova.API.DTOs;

public record MedicalRecordResponse(
    Guid Id,
    Guid StudentId,
    string Anamnesis,
    string ClinicalNotes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
