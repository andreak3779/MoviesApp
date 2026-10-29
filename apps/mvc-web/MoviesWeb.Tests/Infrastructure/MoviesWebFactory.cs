using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoviesWeb.Models;
using MoviesWeb.Services;

namespace MoviesWeb.Tests.Infrastructure;

/// <summary>
/// Boots the real MVC app in-process for integration tests.
/// </summary>
/// <remarks>
/// The app starts exactly as it starts in production: the same
/// <c>Program.cs</c> pipeline, the same middleware order, the same Razor
/// views. Only the two API clients are replaced, so the tests exercise the
/// views and controllers that ship.
/// </remarks>
public class MoviesWebFactory : WebApplicationFactory<Program>
{
    /// <summary>The movies the stub API hands back.</summary>
    public static IReadOnlyList<Movie> Movies { get; } = new List<Movie>
    {
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Inception",
            ReleaseYear = 2010,
            Description = "A mind-bending thriller.",
            Favourite = true,
            Criterion = true,
            CriterionNumber = 42,
            OwnerId = "user-1",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Genre = "Science Fiction",
            MediaType = "Blu-Ray",
            Format = "Movie",
            FilmGenres = new List<FilmGenreDto> { new() { Id = 6, Name = "Science Fiction" } },
            MediaTypes = new List<MediaTypeDto> { new() { Id = 2, Name = "Blu-Ray" } },
            Formats = new List<FormatDto> { new() { Id = 1, Name = "Movie" } }
        },
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Dune",
            ReleaseYear = 2021,
            Description = "Spice on Arrakis.",
            OwnerId = "user-1",
            CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Genre = "Science Fiction",
            MediaType = "4K UHD",
            Format = "Movie"
        }
    };

    /// <summary>The genres the stub API offers.</summary>
    public static IReadOnlyList<FilmGenreDto> Genres { get; } = new List<FilmGenreDto>
    {
        new() { Id = 6, Name = "Science Fiction" },
        new() { Id = 1, Name = "Drama" }
    };

    /// <summary>The media types the stub API offers.</summary>
    public static IReadOnlyList<MediaTypeDto> MediaTypes { get; } = new List<MediaTypeDto>
    {
        new() { Id = 2, Name = "Blu-Ray" },
        new() { Id = 3, Name = "4K UHD" }
    };

    /// <summary>The formats the stub API offers.</summary>
    public static IReadOnlyList<FormatDto> Formats { get; } = new List<FormatDto>
    {
        new() { Id = 1, Name = "Movie" },
        new() { Id = 2, Name = "TV" }
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // The app refuses to start without it. The API clients are
                // replaced below, so nothing is ever sent to this address.
                ["MoviesApi:BaseUrl"] = "http://api.test/api/v1"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMoviesService>();
            services.AddSingleton<IMoviesService, StubMoviesService>();

            services.RemoveAll<IApiAuthService>();
            services.AddSingleton<IApiAuthService, StubApiAuthService>();

            // The movies pages are behind [Authorize]. The test scheme signs
            // every caller in, so the tests can reach the views; the
            // sign-in and sign-out paths have their own tests.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                    options.DefaultSignInScheme = TestAuthHandler.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.Scheme,
                    _ => { });
        });
    }
}

/// <summary>
/// Signs every request in, so the [Authorize] pages can be reached.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>The name of the test authentication scheme.</summary>
    public new const string Scheme = "Test";

    public TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Name, "ada@example.com"),
            new Claim(ClaimTypes.Email, "ada@example.com")
        }, Scheme);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

/// <summary>
/// Stands in for the Web API's movie endpoints.
/// </summary>
public class StubMoviesService : IMoviesService
{
    public Task<PagedResponse<Movie>> GetMoviesAsync(
        MovieQuery? query = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResponse<Movie>
        {
            Items = MoviesWebFactory.Movies.ToList(),
            PageNumber = 1,
            PageSize = 20,
            TotalCount = MoviesWebFactory.Movies.Count,
            TotalPages = 1,
            HasPrevious = false,
            HasNext = false
        });

    public Task<Movie?> GetMovieAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(MoviesWebFactory.Movies.FirstOrDefault(m => m.Id == id));

    public Task DeleteMovieAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpdateMovieAsync(
        Guid id,
        MovieUpdateRequest request,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<Movie?> CreateMovieAsync(
        MovieCreateRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<Movie?>(null);

    public Task<IReadOnlyList<FilmGenreDto>> GetFilmGenresAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(MoviesWebFactory.Genres);

    public Task<IReadOnlyList<MediaTypeDto>> GetMediaTypesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(MoviesWebFactory.MediaTypes);

    public Task<IReadOnlyList<FormatDto>> GetFormatsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(MoviesWebFactory.Formats);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

/// <summary>
/// Stands in for the Web API's auth endpoints.
/// </summary>
public class StubApiAuthService : IApiAuthService
{
    public Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult<AuthResponse?>(new AuthResponse
        {
            Token = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Email = request.Email
        });

    public Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult<AuthResponse?>(new AuthResponse
        {
            Token = "access-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Email = request.Email
        });

    public Task<AuthResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult<AuthResponse?>(new AuthResponse
        {
            Token = "new-access-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            Email = "ada@example.com"
        });

    public Task<bool> LogoutAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}
