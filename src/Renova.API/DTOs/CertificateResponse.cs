namespace Renova.API.DTOs;

public record CertificateResponse(
    Guid Id,
    Guid StudentId,
    Guid CourseId,
    string VerificationCode,
    DateTime IssuedAt);
