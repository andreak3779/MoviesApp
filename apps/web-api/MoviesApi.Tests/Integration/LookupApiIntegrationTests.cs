using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MoviesApi.DTOs;
using MoviesApi.Tests.Infrastructure;
using Xunit;

namespace MoviesApi.Tests.Integration;

/// <summary>
/// The three lookup resources - film genres, media types and formats - share
/// the same shape and the same controller pattern. They are the vocabulary
/// the movie form is built from, so their CRUD surface is exercised here
/// end to end against the real API.
///
/// Each test uses a freshly created row rather than one of the seeded rows,
/// so the tests do not interfere with each other and cannot delete data the
/// application depends on.
/// </summary>
public class LookupApiIntegrationTests : IClassFixture<MoviesApiFactory>
{
    private readonly MoviesApiFactory _factory;

    public LookupApiIntegrationTests(MoviesApiFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// The route segment of every lookup resource.
    /// </summary>
    public static TheoryData<string> LookupRoutes => new()
    {
        "FilmGenres",
        "MediaTypes",
        "Formats"
    };

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task GetAll_ReturnsTheSeededValues(string route)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/{route}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.NotEmpty(items.EnumerateArray());
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task GetById_ReturnsTheRow(string route)
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateAsync(client, route, "Temporary " + route);

        var response = await client.GetAsync($"/api/v1/{route}/{created}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(created, item.GetProperty("id").GetInt32());
        Assert.Equal("Temporary " + route, item.GetProperty("name").GetString());
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task GetById_ForUnknownId_IsNotFound(string route)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/{route}/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task Create_ThenList_IncludesTheNewRow(string route)
    {
        var client = await AuthenticatedClientAsync();
        var name = "Created " + route + " " + Guid.NewGuid().ToString("N")[..6];

        var created = await CreateAsync(client, route, name);

        var items = await (await client.GetAsync($"/api/v1/{route}"))
            .Content.ReadFromJsonAsync<JsonElement>();

        Assert.Contains(
            items.EnumerateArray(),
            item => item.GetProperty("id").GetInt32() == created);
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task Update_RenamesTheRow(string route)
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateAsync(client, route, "Before " + route);
        var renamed = "After " + route;

        var response = await client.PutAsJsonAsync(
            $"/api/v1/{route}/{created}",
            new { name = renamed, description = "Updated by the test." });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var item = await (await client.GetAsync($"/api/v1/{route}/{created}"))
            .Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(renamed, item.GetProperty("name").GetString());
        Assert.Equal("Updated by the test.", item.GetProperty("description").GetString());
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task Update_ForUnknownId_IsNotFound(string route)
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PutAsJsonAsync(
            $"/api/v1/{route}/{int.MaxValue}",
            new { name = "Nothing", description = "Nothing" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task Delete_RemovesTheRow(string route)
    {
        var client = await AuthenticatedClientAsync();
        var created = await CreateAsync(client, route, "Doomed " + route);

        var response = await client.DeleteAsync($"/api/v1/{route}/{created}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var reread = await client.GetAsync($"/api/v1/{route}/{created}");
        Assert.Equal(HttpStatusCode.NotFound, reread.StatusCode);
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task Delete_ForUnknownId_IsNotFound(string route)
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.DeleteAsync($"/api/v1/{route}/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task Writes_WithoutAToken_AreUnauthorized(string route)
    {
        var client = _factory.CreateClient();

        var create = await client.PostAsJsonAsync(
            $"/api/v1/{route}",
            new { name = "Anonymous" });
        Assert.Equal(HttpStatusCode.Unauthorized, create.StatusCode);

        var update = await client.PutAsJsonAsync(
            $"/api/v1/{route}/{int.MaxValue}",
            new { name = "Anonymous" });
        Assert.Equal(HttpStatusCode.Unauthorized, update.StatusCode);

        var delete = await client.DeleteAsync($"/api/v1/{route}/{int.MaxValue}");
        Assert.Equal(HttpStatusCode.Unauthorized, delete.StatusCode);
    }

    [Theory]
    [MemberData(nameof(LookupRoutes))]
    public async Task Create_WithoutAName_IsRejected(string route)
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync($"/api/v1/{route}", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- helpers -------------------------------------------------------

    /// <summary>
    /// Creates a row in the given lookup and returns its id.
    /// </summary>
    private static async Task<int> CreateAsync(HttpClient client, string route, string name)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/v1/{route}",
            new { name, description = $"Created by the {route} test." });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        return created.GetProperty("id").GetInt32();
    }

    private async Task<HttpClient> AuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/register", new
        {
            email = $"lookup-{Guid.NewGuid():N}@moviesapp.test",
            password = "Passw0rd!2026",
            confirmPassword = "Passw0rd!2026",
            firstName = "Lookup",
            lastName = "Tester"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }
}
