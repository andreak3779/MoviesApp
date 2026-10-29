using System.Net;
using MoviesWeb.Tests.Infrastructure;

namespace MoviesWeb.Tests.Pages;

/// <summary>
/// Verifies that the movie pages render the mockup design against the real
/// Razor views: the sidebar shell, the card grid, the form and the details
/// page.
/// </summary>
public class MoviesPageTests : IClassFixture<MoviesWebFactory>
{
    private readonly MoviesWebFactory _factory;

    public MoviesPageTests(MoviesWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MoviesList_RendersTheCardGrid()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Movies");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("movie-grid", html);
        Assert.Contains("movie-card", html);
        Assert.Contains("Inception", html);
        Assert.Contains("Dune", html);
    }

    [Fact]
    public async Task MoviesList_RendersTheSidebarShell()
    {
        var client = _factory.CreateClient();

        var html = await (await client.GetAsync("/Movies")).Content.ReadAsStringAsync();

        Assert.Contains("<aside>", html);
        Assert.Contains("MoviesApp", html);
        Assert.Contains("Sign out", html);
    }

    [Fact]
    public async Task MoviesList_RendersTheTile_Badges_AndPager()
    {
        var client = _factory.CreateClient();

        var html = await (await client.GetAsync("/Movies")).Content.ReadAsStringAsync();

        Assert.Contains("class=\"tile\"", html);
        Assert.Contains("oklch(", html);
        Assert.Contains("CRITERION #42", html);
        Assert.Contains("★ Favourite", html);
        Assert.Contains("Page 1 of 1", html);
    }

    [Fact]
    public async Task MoviesList_RendersTheFilters()
    {
        var client = _factory.CreateClient();

        var html = await (await client.GetAsync("/Movies")).Content.ReadAsStringAsync();

        Assert.Contains("name=\"search\"", html);
        Assert.Contains("name=\"genre\"", html);
        Assert.Contains("name=\"format\"", html);
        Assert.Contains("name=\"mediaType\"", html);
        Assert.Contains("name=\"sortBy\"", html);
        Assert.Contains("Science Fiction", html);
        Assert.Contains("Blu-Ray", html);
    }

    [Fact]
    public async Task AddMovieForm_RendersTheEmptyForm()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Movies/Create");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Add Movie", html);
        Assert.Contains("name=\"Title\"", html);
        Assert.Contains("name=\"ReleaseYear\"", html);
        Assert.Contains("Criterion Collection", html);
        Assert.Contains("Save Movie", html);
    }

    [Fact]
    public async Task EditMovieForm_RendersTheMovieThatWasLoaded()
    {
        var client = _factory.CreateClient();
        var id = MoviesWebFactory.Movies[0].Id;

        var response = await client.GetAsync($"/Movies/Edit/{id}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Edit Movie", html);
        Assert.Contains("value=\"Inception\"", html);
        Assert.Contains("value=\"2010\"", html);
    }

    [Fact]
    public async Task MovieDetails_RendersTheDescription()
    {
        var client = _factory.CreateClient();
        var id = MoviesWebFactory.Movies[0].Id;

        var response = await client.GetAsync($"/Movies/MovieDetails/{id}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("A mind-bending thriller.", html);
        Assert.Contains("detail-list", html);
        Assert.Contains("Release year", html);
    }

    [Fact]
    public async Task MovieDetails_ReturnsNotFound_ForAnUnknownMovie()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Movies/MovieDetails/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
