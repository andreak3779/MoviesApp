using System.Text.Json;

namespace MoviesApi.Services;

/// <summary>
/// Strips password, token, and similar high-risk fields from arbitrary
/// JSON payloads before they are persisted into <c>AuditLog.NewValues</c>
/// / <c>OldValues</c>. See SECURITY-INCIDENT-2026-09-14.md for context.
/// </summary>
public static class SensitiveFieldRedactor
{
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "currentpassword",
        "newpassword",
        "confirmpassword",
        "token",
        "accesstoken",
        "refreshtoken",
        "secret",
        "secretkey",
        "apikey",
    };

    /// <summary>
    /// Path prefixes for which the entire body is dropped (not redacted)
    /// even if no field matched — defence in depth for the auth surface.
    /// </summary>
    private static readonly string[] SensitivePaths =
    {
        "/api/v1/Auth/login",
        "/api/v1/Auth/register",
        "/api/v1/Auth/refresh",
        "/api/v1/Auth/logout",
    };

    public static bool ShouldDropBody(string? path) =>
        !string.IsNullOrEmpty(path) &&
        SensitivePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    public static object? Redact(object? payload)
    {
        if (payload is null) return null;
        try
        {
            // Round-trip via JsonElement so we can mutate arbitrary structure.
            using var doc = JsonSerializer.SerializeToDocument(payload);
            var cloned = WalkElement(doc.RootElement);
            return cloned;
        }
        catch
        {
            // If the payload is opaque (e.g. a stream we already consumed)
            // fall back to a typed placeholder rather than persisting PII.
            return "[unredactable payload]";
        }
    }

    private static object? WalkElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    if (SensitiveFields.Contains(prop.Name))
                    {
                        dict[prop.Name] = "[redacted]";
                    }
                    else
                    {
                        dict[prop.Name] = WalkElement(prop.Value);
                    }
                }
                return dict;
            }
            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(WalkElement(item));
                }
                return list;
            }
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out var l)) return l;
                if (element.TryGetDouble(out var d)) return d;
                return element.GetRawText();
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
                return null;
            default:
                return element.GetRawText();
        }
    }
}
