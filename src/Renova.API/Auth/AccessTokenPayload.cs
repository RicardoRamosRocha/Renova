namespace Renova.API.Auth;

public record AccessTokenPayload(
    string UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    long ExpiresAt);
