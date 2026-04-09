using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(AppUser user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, object?>
        {
            ["sub"] = user.Id,
            ["name"] = user.FullName,
            ["email"] = user.Email,
            ["role"] = user.Role,
            ["iss"] = _settings.Issuer,
            ["aud"] = _settings.Audience,
            ["iat"] = ToUnixTimeSeconds(DateTime.UtcNow),
            ["exp"] = ToUnixTimeSeconds(expiresAtUtc),
            ["jti"] = Guid.NewGuid()
        }));

        var unsignedToken = $"{header}.{payload}";
        var signature = Base64UrlEncode(Sign(unsignedToken));
        return ($"{unsignedToken}.{signature}", expiresAtUtc);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var segments = token.Split('.');
        if (segments.Length != 3)
        {
            return null;
        }

        var unsignedToken = $"{segments[0]}.{segments[1]}";
        var expectedSignature = Sign(unsignedToken);
        var actualSignature = Base64UrlDecode(segments[2]);
        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature))
        {
            return null;
        }

        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(segments[1]));
        using var document = JsonDocument.Parse(payloadJson);
        var root = document.RootElement;

        if (!root.TryGetProperty("exp", out var expElement) || DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64()) <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, root.GetProperty("sub").ToString()),
            new(ClaimTypes.Name, root.GetProperty("name").GetString() ?? string.Empty),
            new(ClaimTypes.Email, root.GetProperty("email").GetString() ?? string.Empty),
            new(ClaimTypes.Role, root.GetProperty("role").GetString() ?? string.Empty)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomJwt"));
    }

    private byte[] Sign(string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.Key));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string data)
    {
        var normalized = data.Replace('-', '+').Replace('_', '/');
        normalized = normalized.PadRight(normalized.Length + (4 - normalized.Length % 4) % 4, '=');
        return Convert.FromBase64String(normalized);
    }

    private static long ToUnixTimeSeconds(DateTime utcDateTime) => new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();
}
