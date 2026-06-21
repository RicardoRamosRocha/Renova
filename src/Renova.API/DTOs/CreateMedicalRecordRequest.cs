using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record CreateMedicalRecordRequest(
    [Required]
    string Anamnesis,

    [Required]
    string ClinicalNotes);
