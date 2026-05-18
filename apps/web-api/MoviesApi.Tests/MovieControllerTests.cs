using Microsoft.AspNetCore.Mvc;
using Moq;
using MoviesApi.Models;
using MoviesApi.Repositories;
using MoviesApi;

namespace MoviesApi.Tests.Controllers;

public class MovieControllerTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly MovieController _controller;

    public MovieControllerTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _controller = new MovieController(_mockRepository.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfMovies()
    {
        var movies = new List<Movie>
        {
            new Movie { Id = 1, Title = "Inception", FilmGenre = "Sci-Fi", ReleaseYear = 2010 },
            new Movie { Id = 2, Title = "The Godfather", FilmGenre = "Crime", ReleaseYear = 1972 }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(movies);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultMovies = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);

        Assert.NotEmpty(resultMovies);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkResult_WithMovie()
    {
        var movie = new Movie { Id = 1, Title = "Inception", FilmGenre = "Sci-Fi", ReleaseYear = 2010 };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultMovie = Assert.IsType<Movie>(okResult.Value);

        Assert.Equal(1, resultMovie.Id);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.GetByIdAsync(int.MaxValue)).ReturnsAsync((Movie?)null);

        var result = await _controller.GetById(int.MaxValue);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionResult_WithCreatedMovie()
    {
        var newMovie = new Movie
        {
            Id = 3,
            Title = "Track B Validation",
            FilmGenre = "Drama",
            ReleaseYear = 2026,
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Movie>())).ReturnsAsync(newMovie);

        var createdResult = await _controller.Create(new Movie
        {
            Title = "Track B Validation",
            FilmGenre = "Drama",
            ReleaseYear = 2026,
        });

        var createdAtAction = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
        var createdMovie = Assert.IsType<Movie>(createdAtAction.Value);

        Assert.Equal("Track B Validation", createdMovie.Title);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsNoContent()
    {
        var existingMovie = new Movie
        {
            Id = 1,
            Title = "Inception",
            FilmGenre = "Sci-Fi",
            ReleaseYear = 2010
        };
        _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<Movie>())).ReturnsAsync(existingMovie);

        var updateResult = await _controller.Update(
            1,
            new Movie
            {
                Title = "Inception Updated",
                FilmGenre = "Sci-Fi",
                ReleaseYear = 2011,
            }
        );

        Assert.IsType<NoContentResult>(updateResult);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.UpdateAsync(int.MaxValue, It.IsAny<Movie>())).ReturnsAsync((Movie?)null);

        var updateResult = await _controller.Update(
            int.MaxValue,
            new Movie
            {
                Title = "Non-existent",
                FilmGenre = "Drama",
                ReleaseYear = 2020,
            }
        );

        Assert.IsType<NotFoundResult>(updateResult);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var deleteResult = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(deleteResult);
    }

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.DeleteAsync(int.MaxValue)).ReturnsAsync(false);

        var deleteResult = await _controller.Delete(int.MaxValue);

        Assert.IsType<NotFoundResult>(deleteResult);
    }
}