using System.ComponentModel.DataAnnotations;

namespace Renova.API.DTOs;

public record UpdateMedicalRecordRequest(
    [Required]
    string Anamnesis,

    [Required]
    string ClinicalNotes);
