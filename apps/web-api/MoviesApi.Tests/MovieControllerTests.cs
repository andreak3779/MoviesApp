using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.Controllers;
using MoviesApi.DTOs;
using MoviesApi.Services;
using System.Security.Claims;

namespace MoviesApi.Tests.Controllers;

public class MovieControllerTests
{
    private readonly Mock<IMovieRepository> _mockRepository;
    private readonly Mock<ILogger<MovieController>> _mockLogger;
    private readonly Mock<IAuditLogService> _mockAuditLogService;
    private readonly MovieController _controller;
    private readonly string _testUserId;

    public MovieControllerTests()
    {
        _mockRepository = new Mock<IMovieRepository>();
        _mockLogger = new Mock<ILogger<MovieController>>();
        _mockAuditLogService = new Mock<IAuditLogService>();
        _controller = new MovieController(_mockRepository.Object, _mockLogger.Object, _mockAuditLogService.Object);
        _testUserId = Guid.NewGuid().ToString();
        
        // Setup HttpContext with user claims
        var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId)
        }, "TestAuth"));
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithListOfMovies()
    {
        var movies = new List<Movie>
        {
            new Movie 
            { 
                Id = Guid.NewGuid(), 
                Title = "Inception", 
                ReleaseYear = 2010,
                OwnerId = _testUserId,
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
        _mockRepository.Setup(r => r.GetPagedByUserAsync(_testUserId, 1, 20))
            .ReturnsAsync((movies, movies.Count));

        var result = await _controller.GetAll(new PaginationParameters { PageNumber = 1, PageSize = 20 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResponse = Assert.IsType<PagedResponse<MovieResponseDto>>(okResult.Value);

        Assert.NotEmpty(pagedResponse.Items);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkResult_WithMovie()
    {
        var movieId = Guid.NewGuid();
        var movie = new Movie 
        { 
            Id = movieId, 
            Title = "Inception", 
            ReleaseYear = 2010,
            OwnerId = _testUserId,
            MovieFilmGenres = new List<MovieFilmGenre>
            {
                new MovieFilmGenre { FilmGenre = new FilmGenre { Id = 1, Name = "Science Fiction" } }
            },
            MovieMediaTypes = new List<MovieMediaType>
            {
                new MovieMediaType { MediaType = new MediaType { Id = 2, Name = "Blu-Ray" } }
            }
        };
        _mockRepository.Setup(r => r.GetByIdWithOwnershipCheckAsync(movieId, _testUserId)).ReturnsAsync(movie);

        var result = await _controller.GetById(movieId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultMovie = Assert.IsType<MovieResponseDto>(okResult.Value);

        Assert.Equal(movieId, resultMovie.Id);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        _mockRepository.Setup(r => r.GetByIdWithOwnershipCheckAsync(movieId, userId)).ReturnsAsync((Movie?)null);

        var result = await _controller.GetById(movieId);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionResult_WithCreatedMovie()
    {
        var userId = Guid.NewGuid().ToString();
        var movieDto = new MovieCreateDto
        {
            Title = "Track B Validation",
            ReleaseYear = 2026,
            MediaTypeIds = new List<int> { 2 },
            FilmGenreIds = new List<int> { 4 }
        };

        var createdMovie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Track B Validation",
            ReleaseYear = 2026,
            OwnerId = userId,
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
        var movieId = Guid.NewGuid();
        var movieDto = new MovieUpdateDto
        {
            Title = "Inception Updated",
            ReleaseYear = 2011,
            MediaTypeIds = new List<int> { 2 },
            FilmGenreIds = new List<int> { 1 }
        };

        var existingMovie = new Movie
        {
            Id = movieId,
            Title = "Inception Updated",
            ReleaseYear = 2011,
            OwnerId = _testUserId,
            MovieFilmGenres = new List<MovieFilmGenre>
            {
                new MovieFilmGenre { FilmGenre = new FilmGenre { Id = 1, Name = "Science Fiction" } }
            },
            MovieMediaTypes = new List<MovieMediaType>
            {
                new MovieMediaType { MediaType = new MediaType { Id = 2, Name = "Blu-Ray" } }
            }
        };
        _mockRepository.Setup(r => r.GetByIdWithOwnershipCheckAsync(movieId, _testUserId)).ReturnsAsync(existingMovie);
        _mockRepository.Setup(r => r.UpdateAsync(movieId, It.IsAny<Movie>())).ReturnsAsync(existingMovie);

        var updateResult = await _controller.Update(movieId, movieDto);

        Assert.IsType<NoContentResult>(updateResult);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNotFound()
    {
        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var movieDto = new MovieUpdateDto
        {
            Title = "Non-existent",
            ReleaseYear = 2020,
            MediaTypeIds = new List<int>(),
            FilmGenreIds = new List<int>()
        };

        _mockRepository.Setup(r => r.UpdateAsync(movieId, It.IsAny<Movie>())).ReturnsAsync((Movie?)null);

        var updateResult = await _controller.Update(movieId, movieDto);

        Assert.IsType<NotFoundResult>(updateResult);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        var movieId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdWithOwnershipCheckAsync(movieId, _testUserId)).ReturnsAsync(new Movie { Id = movieId, OwnerId = _testUserId, Title = "Test Movie", ReleaseYear = 2020 });
        _mockRepository.Setup(r => r.DeleteWithOwnershipCheckAsync(movieId, _testUserId)).ReturnsAsync(true);

        var deleteResult = await _controller.Delete(movieId);

        Assert.IsType<NoContentResult>(deleteResult);
    }

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        var movieId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        _mockRepository.Setup(r => r.DeleteAsync(movieId)).ReturnsAsync(false);

        var deleteResult = await _controller.Delete(movieId);

        Assert.IsType<NotFoundResult>(deleteResult);
    }
}