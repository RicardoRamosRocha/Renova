namespace Renova.API.DTOs;

public record UpdateStudentRequest(
    string FullName,
    DateTime BirthDate,
    string CPF,
    string Phone,
    string? Email,
    string? Address,
    int Status,
    DateTime AdmissionDate);