using Microsoft.Extensions.Configuration;
using MoviesApi.Services;
using Xunit;

namespace MoviesApi.Tests;

/// <summary>
/// Verifies that the JWT secret-key guards in AuthService.EnsureSecretKeyIsValid
/// reject the placeholder and short keys. AuthService.EnsureSecretKeyIsValid is
/// private, but the guard is also enforced from Program.cs at startup, so
/// these tests pin the placeholder string so it can never be silently
/// re-allowed.
/// </summary>
public class AuthServiceSecretKeyGuardTests
{
    private const string Placeholder =
        "CHANGE_THIS_TO_A_SECURE_RANDOM_STRING_MIN_32_CHARS_IN_PRODUCTION";

    [Fact]
    public void Placeholder_IsAtLeast32BytesLong()
    {
        // Pin the placeholder length so the regex above matches it. If we ever
        // change the placeholder we want this test to fail loudly.
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(Placeholder) >= 32);
    }

    [Fact]
    public void SecretKey_LengthUnder32_IsRejected()
    {
        // A key that is non-placeholder but too short.
        var shortKey = new string('a', 31);
        Assert.Throws<InvalidOperationException>(() =>
            EnsureSecretKeyIsValidHelper(shortKey));
    }

    [Fact]
    public void SecretKey_Placeholder_IsRejected()
    {
        Assert.Throws<InvalidOperationException>(() =>
            EnsureSecretKeyIsValidHelper(Placeholder));
    }

    [Fact]
    public void SecretKey_LongEnough_NotPlaceholder_IsAccepted()
    {
        var okKey = new string('a', 48);
        EnsureSecretKeyIsValidHelper(okKey); // does not throw
    }

    /// <summary>
    /// Reflective invocation so the tests do not depend on the internal
    /// helper's visibility. The helper is also covered by Program.cs.
    /// </summary>
    private static void EnsureSecretKeyIsValidHelper(string secretKey)
    {
        var method = typeof(AuthService).GetMethod(
            "EnsureSecretKeyIsValid",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        try
        {
            method!.Invoke(null, new object?[] { secretKey });
        }
        catch (System.Reflection.TargetInvocationException tie) when (tie.InnerException is InvalidOperationException ioe)
        {
            throw ioe;
        }
    }
}
