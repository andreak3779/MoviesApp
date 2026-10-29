using Microsoft.Extensions.Configuration;
using Xunit;

namespace MoviesApi.Tests.Configuration;

/// <summary>
/// Documents how the CORS origin list binds from the environment, so the
/// docker-compose wiring is backed by an assertion rather than by assumption.
/// </summary>
public class ConfigurationBindingTests
{
    [Fact]
    public void AllowedOrigins_BindsFromIndexedEnvironmentVariables()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost:4000",
                ["Cors:AllowedOrigins:1"] = "http://localhost:8080",
            })
            .Build();

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        Assert.NotNull(origins);
        Assert.Equal(new[] { "http://localhost:4000", "http://localhost:8080" }, origins);
    }

    [Fact]
    public void AllowedOrigins_CommaSeparatedValue_DoesNotBindToAnArray()
    {
        // A comma-separated environment variable does NOT bind to a string
        // array: the binder sees the single key "Cors:AllowedOrigins" and
        // returns null. docker-compose therefore has to supply the list as
        // indexed variables ("Cors__AllowedOrigins__0", "...__1"), which is
        // what the compose file does.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins"] = "http://localhost:4000,http://localhost:8080",
            })
            .Build();

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        Assert.Null(origins);
    }
}
