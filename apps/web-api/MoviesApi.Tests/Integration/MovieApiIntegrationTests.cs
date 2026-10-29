using System.Net;
using System.Net.Http.Json;
using MoviesApi.DTOs;
using MoviesApi.Tests.Infrastructure;
using Xunit;

namespace MoviesApi.Tests.Integration;

/// <summary>
/// Boots the real API with <see cref="MoviesApiFactory"/> and exercises the
/// contract a client actually depends on: register, sign in, then create,
/// read, update and delete a movie.
///
/// These tests run against the same pipeline, the same middleware order and
/// the same EF Core setup as production, so a regression in any of them
/// fails here.
/// </summary>
public class MovieApiIntegrationTests : IClassFixture<MoviesApiFactory>
{
    private const string Password = "Passw0rd!2026";

    private readonly MoviesApiFactory _factory;

    public MovieApiIntegrationTests(MoviesApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ReturnsTokens()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            email = UniqueEmail(),
            password = Password,
            confirmPassword = Password,
            firstName = "Ada",
            lastName = "Lovelace"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        Assert.Equal("Ada", auth.FirstName);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var client = _factory.CreateClient();
        var email = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { email, password = Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_IsUnauthorized()
    {
        var client = _factory.CreateClient();
        var email = await RegisterAsync(client);

        var response = await client.PostAsJsonAsync(
            "/api/v1/Auth/login",
            new { email, password = "Wr0ng!Password" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMovies_WithoutToken_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/Movie");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateMovie_ThenList_ReturnsTheMovie()
    {
        var client = await AuthenticatedClientAsync();

        var created = await CreateMovieAsync(client, "Interstellar", 2014);

        Assert.Equal("Interstellar", created.Title);
        Assert.Equal(2014, created.ReleaseYear);
        Assert.NotEqual(Guid.Empty, created.Id);

        var paged = await GetMoviesAsync(client);

        Assert.Contains(paged.Items, m => m.Title == "Interstellar");
    }

    [Fact]
    public async Task CreateMovie_WithFormatId_ReturnsTheFormatHydrated()
    {
        // Regression test for defect G25: the repository used to omit the
        // MovieFormats include when reading a movie back, so the response
        // dereferenced a null Format and the endpoint returned a 500.
        var client = await AuthenticatedClientAsync();

        var created = await CreateMovieAsync(client, "Box Set Movie", 2020, formatId: 3);

        Assert.NotNull(created.Format);
        Assert.Equal("BoxSet", created.Format);
        Assert.Contains(created.Formats, f => f.Id == 3);
    }

    [Fact]
    public async Task GetMovie_ById_ReturnsTheMovie()
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateMovieAsync(client, "The Matrix", 1999);

        var response = await client.GetAsync($"/api/v1/Movie/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var movie = await response.Content.ReadFromJsonAsync<MovieResponseDto>();
        Assert.Equal("The Matrix", movie!.Title);
    }

    [Fact]
    public async Task GetMovie_ByUnknownId_IsNotFound()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/v1/Movie/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMovie_PersistsEveryField()
    {
        // Regression test for defect G26: the repository used to copy only
        // the title, the year and the link collections, silently discarding
        // the description, the favourite flag, the criterion fields and the
        // formats.
        var client = await AuthenticatedClientAsync();
        var created = await CreateMovieAsync(client, "Working Title", 2001, formatId: 1);

        var update = new MovieUpdateDto
        {
            Title = "Corrected Title",
            ReleaseYear = 2002,
            Description = "Now with a description.",
            Favourite = true,
            Criterion = true,
            CriterionNumber = 7,
            FormatId = 2
        };

        var response = await client.PutAsJsonAsync($"/api/v1/Movie/{created.Id}", update);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var reread = await client.GetFromJsonAsync<MovieResponseDto>($"/api/v1/Movie/{created.Id}");

        Assert.Equal("Corrected Title", reread!.Title);
        Assert.Equal(2002, reread.ReleaseYear);
        Assert.Equal("Now with a description.", reread.Description);
        Assert.True(reread.Favourite);
        Assert.True(reread.Criterion);
        Assert.Equal(7, reread.CriterionNumber);
        Assert.Equal("TVSeries", reread.Format);
    }

    [Fact]
    public async Task DeleteMovie_RemovesItFromTheCollection()
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateMovieAsync(client, "Doomed", 1999);

        var response = await client.DeleteAsync($"/api/v1/Movie/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var reread = await client.GetAsync($"/api/v1/Movie/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, reread.StatusCode);
    }

    [Fact]
    public async Task Movies_AreScopedToTheirOwner()
    {
        var firstClient = await AuthenticatedClientAsync();
        var secondClient = await AuthenticatedClientAsync();

        await CreateMovieAsync(firstClient, "Private Movie", 1999);

        var paged = await GetMoviesAsync(secondClient);

        Assert.DoesNotContain(paged.Items, m => m.Title == "Private Movie");
    }

    [Fact]
    public async Task GetMovies_SupportsSearchFilteringAndPaging()
    {
        var client = await AuthenticatedClientAsync();

        await CreateMovieAsync(client, "Alien", 1979, genreId: 6);
        await CreateMovieAsync(client, "Aliens", 1986, genreId: 6);
        await CreateMovieAsync(client, "Amelie", 2001, genreId: 3);

        var search = await GetMoviesAsync(client, new Dictionary<string, string>
        {
            ["search"] = "alie"
        });
        Assert.Equal(2, search.Items.Count);

        var filtered = await GetMoviesAsync(client, new Dictionary<string, string>
        {
            ["genre"] = "Comedy"
        });
        Assert.Contains(filtered.Items, m => m.Title == "Amelie");
        Assert.DoesNotContain(filtered.Items, m => m.Title == "Alien");

        var paged = await GetMoviesAsync(client, new Dictionary<string, string>
        {
            ["pageNumber"] = "1",
            ["pageSize"] = "1"
        });
        Assert.Single(paged.Items);
        Assert.Equal(3, paged.TotalCount);
        Assert.True(paged.HasNext);
    }

    [Fact]
    public async Task GetLookups_ReturnsTheSeededValues()
    {
        var client = _factory.CreateClient();

        var genres = await client.GetFromJsonAsync<List<FilmGenreDto>>("/api/v1/FilmGenres");
        var mediaTypes = await client.GetFromJsonAsync<List<MediaTypeDto>>("/api/v1/MediaTypes");
        var formats = await client.GetFromJsonAsync<List<FormatDto>>("/api/v1/Formats");

        Assert.Contains(genres!, g => g.Name == "Science Fiction");
        Assert.Contains(mediaTypes!, m => m.Name == "Blu-Ray");
        Assert.Contains(formats!, f => f.Name == "BoxSet");
    }

    // ---- helpers -------------------------------------------------------

    private static string UniqueEmail() =>
        $"test-{Guid.NewGuid():N}@moviesapp.test";

    /// <summary>
    /// Creates a client with a freshly registered account and a bearer token
    /// already attached.
    /// </summary>
    private async Task<HttpClient> AuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        await RegisterAsync(client);
        return client;
    }

    private async Task<string> RegisterAsync(HttpClient client)
    {
        var email = UniqueEmail();
        var response = await client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            email,
            password = Password,
            confirmPassword = Password,
            firstName = "Test",
            lastName = "User"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);

        return email;
    }

    private static async Task<MovieResponseDto> CreateMovieAsync(
        HttpClient client,
        string title,
        int releaseYear,
        int? genreId = null,
        int? formatId = null)
    {
        var payload = new MovieCreateDto
        {
            Title = title,
            ReleaseYear = releaseYear,
            Description = $"{title} description.",
            Favourite = false,
            Criterion = false,
            FilmGenreId = genreId,
            FormatId = formatId
        };

        var response = await client.PostAsJsonAsync("/api/v1/Movie", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<MovieResponseDto>();
        return created!;
    }

    private static async Task<PagedResponse<MovieResponseDto>> GetMoviesAsync(
        HttpClient client,
        Dictionary<string, string>? query = null)
    {
        var url = "/api/v1/Movie";
        if (query is { Count: > 0 })
        {
            var pairs = query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
            url += "?" + string.Join("&", pairs);
        }

        var paged = await client.GetFromJsonAsync<PagedResponse<MovieResponseDto>>(url);
        return paged!;
    }
}
