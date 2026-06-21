namespace Renova.API.DTOs;

public record CreateStudentRequest(
    string FullName,
    DateTime BirthDate,
    string CPF,
    string Phone,
    string? Email,
    string? Address,
    int Status,
    DateTime AdmissionDate);