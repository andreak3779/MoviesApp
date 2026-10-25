using System.Net;
using System.Text;
using System.Text.Json;
using MoviesWeb.Models;
using MoviesWeb.Services;

namespace MoviesWeb.Tests.Services;

/// <summary>
/// Verifies that <see cref="MoviesService"/> talks to the Web API using the
/// routes and shapes the API actually exposes:
///   - movies live at <c>/api/v1/Movie</c> (singular controller name)
///   - movies are identified by a GUID
///   - the list endpoint returns a paged envelope, not a bare array
/// </summary>
public class MoviesServiceTests
{
    private const string BaseUrl = "http://api.test/api/v1/";

    private readonly Mock<HttpMessageHandler> _handler = new(MockBehavior.Strict);

    private readonly List<HttpRequestMessage> _requests = new();

    private MoviesService CreateService() =>
        new(new HttpClient(_handler.Object) { BaseAddress = new Uri(BaseUrl) });

    private void Respond(HttpStatusCode status, string json)
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => _requests.Add(request))
            .ReturnsAsync(() => new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
    }

    /// <summary>Gets the single request that was sent to the API.</summary>
    private HttpRequestMessage SingleRequest() => Assert.Single(_requests);

    [Fact]
    public async Task GetMoviesAsync_UsesSingularMovieRoute_AndReadsPagedEnvelope()
    {
        Respond(HttpStatusCode.OK, """
            {
              "items": [
                {
                  "id": "11111111-1111-1111-1111-111111111111",
                  "title": "Inception",
                  "releaseYear": 2010,
                  "description": "A mind-bending thriller.",
                  "favourite": true,
                  "criterion": true,
                  "criterionNumber": 42,
                  "ownerId": "user-1",
                  "createdAt": "2026-01-01T00:00:00Z",
                  "updatedAt": null,
                  "genre": "Science Fiction",
                  "mediaType": "Blu-Ray",
                  "format": "Movie",
                  "mediaTypes": [],
                  "filmGenres": [],
                  "formats": []
                }
              ],
              "pageNumber": 1,
              "pageSize": 20,
              "totalCount": 1,
              "totalPages": 1,
              "hasPrevious": false,
              "hasNext": false
            }
            """);

        var service = CreateService();
        var page = await service.GetMoviesAsync();

        var movie = Assert.Single(page.Items);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), movie.Id);
        Assert.Equal("Inception", movie.Title);
        Assert.Equal(2010, movie.ReleaseYear);
        Assert.True(movie.Favourite);
        Assert.True(movie.Criterion);
        Assert.Equal(42, movie.CriterionNumber);
        Assert.Equal("Science Fiction", movie.Genre);
        Assert.Equal(1, page.TotalCount);
        Assert.False(page.HasNext);

        var requestUri = SingleRequest().RequestUri!;
        Assert.Equal("/api/v1/Movie", requestUri.AbsolutePath);
    }

    [Fact]
    public async Task GetMoviesAsync_PassesFiltersAsQueryString()
    {
        Respond(HttpStatusCode.OK, """{"items":[],"pageNumber":1,"pageSize":20,"totalCount":0,"totalPages":0,"hasPrevious":false,"hasNext":false}""");

        var query = new MovieQuery();
        query.Add("search", "ince");
        query.Add("genre", "Science Fiction");
        query.Add("sortBy", "title");
        query.Add("pageNumber", "2");

        var service = CreateService();
        await service.GetMoviesAsync(query);

        var requestUri = SingleRequest().RequestUri!;

        Assert.Equal("/api/v1/Movie", requestUri.AbsolutePath);
        var qs = requestUri.Query;
        Assert.Contains("search=ince", qs);
        Assert.Contains("genre=Science%20Fiction", qs);
        Assert.Contains("sortBy=title", qs);
        Assert.Contains("pageNumber=2", qs);
    }

    [Fact]
    public async Task GetMovieAsync_UsesGuidInRoute_AndMapsResponse()
    {
        var id = Guid.NewGuid();
        Respond(HttpStatusCode.OK, $$"""
            {
              "id": "{{id}}",
              "title": "Inception",
              "releaseYear": 2010,
              "favourite": false,
              "criterion": false,
              "ownerId": "user-1",
              "createdAt": "2026-01-01T00:00:00Z",
              "updatedAt": null,
              "genre": "Science Fiction",
              "mediaType": "Blu-Ray",
              "format": null,
              "mediaTypes": [],
              "filmGenres": [],
              "formats": []
            }
            """);

        var service = CreateService();
        var movie = await service.GetMovieAsync(id);

        Assert.NotNull(movie);
        Assert.Equal(id, movie.Id);
        Assert.Equal("Inception", movie.Title);

        var requestUri = SingleRequest().RequestUri!;
        Assert.Equal($"/api/v1/Movie/{id}", requestUri.AbsolutePath);
    }

    [Fact]
    public async Task GetMovieAsync_ReturnsNull_WhenApiResponds404()
    {
        Respond(HttpStatusCode.NotFound, """{"error":"Not found"}""");

        var service = CreateService();
        var movie = await service.GetMovieAsync(Guid.NewGuid());

        Assert.Null(movie);
    }

    [Fact]
    public async Task DeleteMovieAsync_TargetsMovieIdRoute()
    {
        Respond(HttpStatusCode.NoContent, string.Empty);

        var id = Guid.NewGuid();
        var service = CreateService();
        await service.DeleteMovieAsync(id);

        var requestUri = SingleRequest().RequestUri!;
        Assert.Equal($"/api/v1/Movie/{id}", requestUri.AbsolutePath);
    }

    [Fact]
    public async Task UpdateMovieAsync_PutsToMovieIdRoute_WithCamelCaseBody()
    {
        Respond(HttpStatusCode.NoContent, string.Empty);

        var id = Guid.NewGuid();
        var movie = new Movie
        {
            Id = id,
            Title = "Inception",
            ReleaseYear = 2010,
            Description = "Updated",
            Favourite = true,
            Criterion = true,
            CriterionNumber = 7,
            FilmGenres = new List<FilmGenreDto> { new() { Id = 6, Name = "Science Fiction" } },
            MediaTypes = new List<MediaTypeDto> { new() { Id = 2, Name = "Blu-Ray" } },
            Formats = new List<FormatDto> { new() { Id = 1, Name = "Movie" } }
        };

        var service = CreateService();
        await service.UpdateMovieAsync(movie);

        var request = SingleRequest();
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"/api/v1/Movie/{id}", request.RequestUri!.AbsolutePath);

        var body = await request.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.Equal("Inception", root.GetProperty("title").GetString());
        Assert.Equal(2010, root.GetProperty("releaseYear").GetInt32());
        Assert.Equal("Updated", root.GetProperty("description").GetString());
        Assert.True(root.GetProperty("favourite").GetBoolean());
        Assert.True(root.GetProperty("criterion").GetBoolean());
        Assert.Equal(7, root.GetProperty("criterionNumber").GetInt32());
        Assert.Equal(6, root.GetProperty("filmGenreId").GetInt32());
        Assert.Equal(2, root.GetProperty("mediaTypeId").GetInt32());
        Assert.Equal(1, root.GetProperty("formatId").GetInt32());
    }

    [Fact]
    public async Task CreateMovieAsync_PostsToMovieCollectionRoute()
    {
        Respond(HttpStatusCode.Created, """{"id":"22222222-2222-2222-2222-222222222222","title":"Inception","releaseYear":2010,"ownerId":"user-1","createdAt":"2026-01-01T00:00:00Z","favourite":false,"criterion":false,"mediaTypes":[],"filmGenres":[],"formats":[]}""");

        var service = CreateService();
        var created = await service.CreateMovieAsync(new Movie
        {
            Title = "Inception",
            ReleaseYear = 2010
        });

        Assert.NotNull(created);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), created.Id);

        var request = SingleRequest();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/api/v1/Movie", request.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetMoviesAsync_ThrowsHttpRequestException_WhenApiFails()
    {
        Respond(HttpStatusCode.InternalServerError, """{"title":"Internal server error"}""");

        var service = CreateService();

        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetMoviesAsync());
        Assert.Contains("500", ex.Message);
    }

    [Fact]
    public async Task GetFilmGenresAsync_MapsLookupList()
    {
        Respond(HttpStatusCode.OK, """[{"id":1,"name":"Action","description":"High-energy"},{"id":2,"name":"Drama","description":null}]""");

        var service = CreateService();
        var genres = await service.GetFilmGenresAsync();

        Assert.Equal(2, genres.Count);
        Assert.Equal("Action", genres[0].Name);
        Assert.Equal(1, genres[0].Id);

        var requestUri = SingleRequest().RequestUri!;
        Assert.Equal("/api/v1/filmGenres", requestUri.AbsolutePath);
    }
}
