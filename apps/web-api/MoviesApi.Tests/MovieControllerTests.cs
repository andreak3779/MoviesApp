using Microsoft.AspNetCore.Mvc;
using Moq;
using MoviesApi.Models;
using MoviesApi.Repositories;
using MoviesApi;
using MoviesApi.DTOs;

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
            new Movie 
            { 
                Id = 1, 
                Title = "Inception", 
                ReleaseYear = 2010,
                MovieFilmGenres = new List<MovieFilmGenre>
                {
                    new MovieFilmGenre { FilmGenre = new FilmGenre { Id = 1, Name = "Science Fiction" } }
                },
                MovieMediaTypes = new List<MovieMediaType>
                {
                    new MovieMediaType { MediaType = new MediaType { Id = 2, Name = "Blu-Ray" } }
                }
            }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(movies);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultMovies = Assert.IsAssignableFrom<IEnumerable<MovieResponseDto>>(okResult.Value);

        Assert.NotEmpty(resultMovies);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkResult_WithMovie()
    {
        var movie = new Movie 
        { 
            Id = 1, 
            Title = "Inception", 
            ReleaseYear = 2010,
            MovieFilmGenres = new List<MovieFilmGenre>
            {
                new MovieFilmGenre { FilmGenre = new FilmGenre { Id = 1, Name = "Science Fiction" } }
            },
            MovieMediaTypes = new List<MovieMediaType>
            {
                new MovieMediaType { MediaType = new MediaType { Id = 2, Name = "Blu-Ray" } }
            }
        };
        _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(movie);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultMovie = Assert.IsType<MovieResponseDto>(okResult.Value);

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
        var movieDto = new MovieDto
        {
            Title = "Track B Validation",
            ReleaseYear = 2026,
            MediaTypeIds = new List<int> { 2 },
            FilmGenreIds = new List<int> { 4 }
        };

        var createdMovie = new Movie
        {
            Id = 3,
            Title = "Track B Validation",
            ReleaseYear = 2026,
            MovieFilmGenres = new List<MovieFilmGenre>
            {
                new MovieFilmGenre { FilmGenre = new FilmGenre { Id = 4, Name = "Drama" } }
            },
            MovieMediaTypes = new List<MovieMediaType>
            {
                new MovieMediaType { MediaType = new MediaType { Id = 2, Name = "Blu-Ray" } }
            }
        };
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Movie>())).ReturnsAsync(createdMovie);

        var createdResult = await _controller.Create(movieDto);

        var createdAtAction = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
        var createdMovieDto = Assert.IsType<MovieResponseDto>(createdAtAction.Value);

        Assert.Equal("Track B Validation", createdMovieDto.Title);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsNoContent()
    {
        var movieDto = new MovieDto
        {
            Title = "Inception Updated",
            ReleaseYear = 2011,
            MediaTypeIds = new List<int> { 2 },
            FilmGenreIds = new List<int> { 1 }
        };

        var existingMovie = new Movie
        {
            Id = 1,
            Title = "Inception Updated",
            ReleaseYear = 2011,
            MovieFilmGenres = new List<MovieFilmGenre>
            {
                new MovieFilmGenre { FilmGenre = new FilmGenre { Id = 1, Name = "Science Fiction" } }
            },
            MovieMediaTypes = new List<MovieMediaType>
            {
                new MovieMediaType { MediaType = new MediaType { Id = 2, Name = "Blu-Ray" } }
            }
        };
        _mockRepository.Setup(r => r.UpdateAsync(1, It.IsAny<Movie>())).ReturnsAsync(existingMovie);

        var updateResult = await _controller.Update(1, movieDto);

        Assert.IsType<NoContentResult>(updateResult);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNotFound()
    {
        var movieDto = new MovieDto
        {
            Title = "Non-existent",
            ReleaseYear = 2020,
            MediaTypeIds = new List<int>(),
            FilmGenreIds = new List<int>()
        };

        _mockRepository.Setup(r => r.UpdateAsync(int.MaxValue, It.IsAny<Movie>())).ReturnsAsync((Movie?)null);

        var updateResult = await _controller.Update(int.MaxValue, movieDto);

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