using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Renova.API.Auth;

public class AccessTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public AccessTokenService(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public string CreateToken(string userId, string email, string fullName, IReadOnlyCollection<string> roles)
    {
        var payload = new AccessTokenPayload(
            userId,
            email,
            fullName,
            roles,
            DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds());

        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadSegment = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var signatureSegment = CreateSignature(payloadSegment);

        return $"{payloadSegment}.{signatureSegment}";
    }

    public AccessTokenPayload? ValidateToken(string token)
    {
        var parts = token.Split('.', 2);

        if (parts.Length != 2)
        {
            return null;
        }

        var expectedSignature = CreateSignature(parts[0]);

        if (!CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(parts[1])))
        {
            return null;
        }

        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
        var payload = JsonSerializer.Deserialize<AccessTokenPayload>(payloadJson);

        if (payload is null || payload.ExpiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            return null;
        }

        return payload;
    }

    private string CreateSignature(string payloadSegment)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(GetSecret()));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadSegment)));
    }

    private string GetSecret()
    {
        var configuredSecret = _configuration["Authentication:TokenSecret"];

        if (!string.IsNullOrWhiteSpace(configuredSecret))
        {
            return configuredSecret;
        }

        if (!_environment.IsDevelopment())
        {
            throw new InvalidOperationException("Authentication:TokenSecret must be configured outside development.");
        }

        return "renova-development-token-secret-change-before-production";
    }

    private static string Base64UrlEncode(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value
            .Replace('-', '+')
            .Replace('_', '/');

        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');

        return Convert.FromBase64String(padded);
    }
}
