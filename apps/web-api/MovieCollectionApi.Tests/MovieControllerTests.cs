using Microsoft.AspNetCore.Mvc;
using MovieCollectionApi;

namespace MovieCollectionApi.Tests.Controllers;

public class MovieControllerTests
{
    private readonly MovieController _controller = new();

    [Fact]
    public void GetAll_ReturnsOkResult_WithListOfMovies()
    {
        var result = _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var movies = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);

        Assert.NotEmpty(movies);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsOkResult_WithMovie()
    {
        var result = _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var movie = Assert.IsType<Movie>(okResult.Value);

        Assert.Equal(1, movie.Id);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNotFound()
    {
        var result = _controller.GetById(int.MaxValue);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void CreateUpdateAndDelete_Flow_Works()
    {
        var createdResult = _controller.Create(new Movie
        {
            Title = "Track B Validation",
            FilmGenre = "Drama",
            ReleaseYear = 2026,
        });

        var createdAtAction = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
        var createdMovie = Assert.IsType<Movie>(createdAtAction.Value);

        var updateResult = _controller.Update(
            createdMovie.Id,
            new Movie
            {
                Title = "Track B Validation Updated",
                FilmGenre = "Drama",
                ReleaseYear = 2027,
            }
        );

        Assert.IsType<NoContentResult>(updateResult);

        var getUpdatedResult = _controller.GetById(createdMovie.Id);
        var updatedOkResult = Assert.IsType<OkObjectResult>(getUpdatedResult.Result);
        var updatedMovie = Assert.IsType<Movie>(updatedOkResult.Value);

        Assert.Equal("Track B Validation Updated", updatedMovie.Title);
        Assert.Equal(2027, updatedMovie.ReleaseYear);

        var deleteResult = _controller.Delete(createdMovie.Id);

        Assert.IsType<NoContentResult>(deleteResult);
        Assert.IsType<NotFoundResult>(_controller.GetById(createdMovie.Id).Result);
    }
}