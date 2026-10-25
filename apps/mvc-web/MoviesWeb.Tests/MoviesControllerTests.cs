using Microsoft.AspNetCore.Mvc;
using MoviesWeb.Controllers;
using MoviesWeb.Models;
using MoviesWeb.Services;
using MoviesWeb.ViewModels;

namespace MoviesWeb.Tests.Controllers;

/// <summary>
/// Verifies that the MVC controller renders the API's paged envelope and
/// navigates by GUID.
/// </summary>
public class MoviesControllerTests
{
    private readonly Mock<IMoviesService> _moviesService = new();
    private readonly Mock<ILogger<MoviesController>> _logger = new();

    private MoviesController CreateController() => new(_moviesService.Object, _logger.Object);

    private static Movie SampleMovie() => new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Title = "Inception",
        ReleaseYear = 2010,
        Description = "A mind-bending thriller.",
        Favourite = true,
        Criterion = false,
        OwnerId = "user-1",
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        Genre = "Science Fiction",
        MediaType = "Blu-Ray",
        Format = "Movie"
    };

    [Fact]
    public async Task Index_ReturnsView_WithPagedModel()
    {
        var movie = SampleMovie();
        _moviesService.Setup(s => s.GetMoviesAsync(It.IsAny<MovieQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<Movie>
            {
                Items = new List<Movie> { movie },
                PageNumber = 1,
                PageSize = 20,
                TotalCount = 1,
                TotalPages = 1
            });
        _moviesService.Setup(s => s.GetFilmGenresAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FilmGenreDto> { new() { Id = 6, Name = "Science Fiction" } });
        _moviesService.Setup(s => s.GetMediaTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MediaTypeDto> { new() { Id = 2, Name = "Blu-Ray" } });
        _moviesService.Setup(s => s.GetFormatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FormatDto> { new() { Id = 1, Name = "Movie" } });

        var controller = CreateController();
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MovieListModel>(viewResult.Model);
        Assert.Single(model.Movies);
        Assert.Equal(1, model.TotalCount);
        Assert.Equal("Science Fiction", model.Genres[0].Name);
        Assert.Equal("Blu-Ray", model.MediaTypes[0].Name);
        Assert.Equal("Movie", model.Formats[0].Name);
    }

    [Fact]
    public async Task Index_PassesFiltersThroughToTheService()
    {
        MovieQuery? captured = null;
        _moviesService.Setup(s => s.GetMoviesAsync(It.IsAny<MovieQuery>(), It.IsAny<CancellationToken>()))
            .Callback<MovieQuery?, CancellationToken>((q, _) => captured = q)
            .ReturnsAsync(new PagedResponse<Movie>());
        _moviesService.Setup(s => s.GetFilmGenresAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FilmGenreDto>());
        _moviesService.Setup(s => s.GetMediaTypesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MediaTypeDto>());
        _moviesService.Setup(s => s.GetFormatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FormatDto>());

        var controller = CreateController();
        await controller.Index(
            search: "ince",
            genre: "Science Fiction",
            mediaType: null,
            format: null,
            sortBy: "title",
            sortDir: "asc",
            page: 3);

        Assert.NotNull(captured);
        var parts = captured.ToQueryStringParts().ToList();
        Assert.Contains("search=ince", parts);
        Assert.Contains("genre=Science%20Fiction", parts);
        Assert.Contains("sortBy=title", parts);
        Assert.Contains("sortDir=asc", parts);
        Assert.Contains("pageNumber=3", parts);
    }

    [Fact]
    public async Task Index_ReturnsViewWithEmptyModel_WhenApiThrows()
    {
        _moviesService.Setup(s => s.GetMoviesAsync(It.IsAny<MovieQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var controller = CreateController();
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MovieListModel>(viewResult.Model);
        Assert.Empty(model.Movies);
        Assert.Equal(0, model.TotalCount);
    }

    [Fact]
    public async Task MovieDetails_ReturnsView_WithMovie()
    {
        var movie = SampleMovie();
        _moviesService.Setup(s => s.GetMovieAsync(movie.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(movie);

        var controller = CreateController();
        var result = await controller.MovieDetails(movie.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Movie>(viewResult.Model);
        Assert.Equal(movie.Id, model.Id);
        Assert.Equal("Inception", model.Title);
    }

    [Fact]
    public async Task MovieDetails_ReturnsNotFound_WhenMovieIsMissing()
    {
        _moviesService.Setup(s => s.GetMovieAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Movie?)null);

        var controller = CreateController();
        var result = await controller.MovieDetails(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_RedirectsToIndex_AndCallsService()
    {
        var id = Guid.NewGuid();
        _moviesService.Setup(s => s.DeleteMovieAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController();
        var result = await controller.Delete(id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        _moviesService.Verify(s => s.DeleteMovieAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
