using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoviesApiData.Data;

namespace MoviesApi.Tests.Infrastructure;

/// <summary>
/// Boots the real API in-process for integration tests.
///
/// The API is started exactly as it starts in production: the same
/// <c>Program.cs</c> pipeline, the same middleware order, the same EF Core
/// setup. Only the environment-specific values are replaced, so a test that
/// passes here exercises the code that ships.
/// </summary>
public class MoviesApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// A throwaway secret that satisfies the API's "at least 32 bytes" guard.
    /// It is never used outside the test host.
    /// </summary>
    private const string JwtSecret = "integration-test-secret-key-at-least-32-bytes-long";

    /// <summary>
    /// A per-factory SQLite file. Each factory instance gets its own, so
    /// tests never share state. A file (not <c>:memory:</c>) is used because
    /// the API opens its own connections to the database.
    /// </summary>
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"moviesapi-tests-{Guid.NewGuid():N}.db");

    /// <summary>
    /// The environment variables the factory overrides, and the values it
    /// found, so they can be put back when the last factory is disposed.
    /// </summary>
    private static readonly Dictionary<string, string?> OriginalEnvironment = new();

    /// <summary>
    /// How many factories are alive. xUnit runs test classes in parallel and
    /// each one gets its own factory, so the environment is only restored
    /// once the last of them has been disposed - otherwise one class could
    /// remove the secret while another is still booting its host.
    /// </summary>
    private static readonly object EnvironmentLock = new();
    private static int _liveFactories;

    public MoviesApiFactory()
    {
        // Program.cs reads several settings while it is still building the
        // host - the JWT secret, the connection string, the rate limits.
        // Configuration added through ConfigureWebHost is only applied once
        // the host is built, which is too late for those reads. Environment
        // variables are read when the host builder is created, so they are
        // the only way to override an eagerly-read setting from a
        // WebApplicationFactory.
        lock (EnvironmentLock)
        {
            if (_liveFactories == 0)
            {
                SetEnvironmentVariable("JwtSettings__SecretKey", JwtSecret);
                SetEnvironmentVariable("EnableHttpsRedirection", "false");
                SetEnvironmentVariable("EnableSwagger", "true");

                // The shipped limits (100 reads / 10 writes per minute) are
                // far too tight for a test suite that issues a request per
                // assertion.
                SetEnvironmentVariable("RateLimiting__GlobalLimit", "100000");
                SetEnvironmentVariable("RateLimiting__WriteLimit", "100000");
            }

            _liveFactories++;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                ["JwtSettings:SecretKey"] = JwtSecret,
                ["EnableHttpsRedirection"] = "false",

                // The Swagger UI is exercised by its own test, so it has to be
                // reachable in the test environment too.
                ["EnableSwagger"] = "true",

                ["RateLimiting:GlobalLimit"] = "100000",
                ["RateLimiting:WriteLimit"] = "100000",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Program.cs reads the connection string while it is registering
            // the DbContext. Re-registering the context here guarantees the
            // test database is the one the running API talks to.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MoviesDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<MoviesDbContext>(options =>
                options.UseSqlite($"Data Source={_databasePath}"));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        lock (EnvironmentLock)
        {
            _liveFactories--;

            if (_liveFactories == 0)
            {
                foreach (var (name, original) in OriginalEnvironment)
                {
                    Environment.SetEnvironmentVariable(name, original);
                }

                OriginalEnvironment.Clear();
            }
        }

        if (File.Exists(_databasePath))
        {
            try { File.Delete(_databasePath); }
            catch (IOException) { /* best effort */ }
        }
    }

    private static void SetEnvironmentVariable(string name, string value)
    {
        OriginalEnvironment[name] = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
    }
}
