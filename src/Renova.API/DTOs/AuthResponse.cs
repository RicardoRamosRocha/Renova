namespace Renova.API.DTOs;

public record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAt,
    UserResponse User);
