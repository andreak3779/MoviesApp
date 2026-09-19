using System;
using MoviesWeb.Models;
using Xunit;

namespace MoviesWeb.Tests;

/// <summary>
/// Verifies that User.Authenticate does not silently authenticate callers.
/// Before this test existed, the body returned <c>true</c> unconditionally
/// — see docs/SECURITY-INCIDENT-2026-09-14.md.
/// </summary>
public class UserAuthenticateTests
{
    [Fact]
    public void Authenticate_ThrowsNotImplemented()
    {
        var user = new User { UserId = 1, Name = "Alice", Email = "alice@example.com" };

        var ex = Assert.Throws<NotImplementedException>(() => user.Authenticate("hunter2"));
        Assert.Contains("Authentication is not implemented", ex.Message);
    }

    [Fact]
    public void Authenticate_DoesNotReturnTrue()
    {
        var user = new User { UserId = 2, Name = "Bob", Email = "bob@example.com" };

        // The contract is now to throw on any call. If anyone reverts to
        // returning a value, this test fails because no exception is
        // thrown at all.
        var threw = false;
        try
        {
            user.Authenticate("anything");
        }
        catch (NotImplementedException)
        {
            threw = true;
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
        }

        Assert.True(threw, "User.Authenticate must throw; never return a value.");
    }
}
