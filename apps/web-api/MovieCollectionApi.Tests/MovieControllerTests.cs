using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using MovieCollectionApi.Controllers;
using MovieCollectionApi.Models;

namespace MovieCollectionApi.Tests.Controllers
{
    public class MovieControllerTest
    {
        private readonly Mock<IList<Movie>> _mockMovies;
        private readonly MovieController _controller;

        public MovieControllerTest()
        {
            // Mock the in-memory movie list
            _mockMovies = new Mock<IList<Movie>>();
            _mockMovies.Setup(m => m.GetEnumerator()).Returns(new List<Movie>
            {
                new Movie { Id = 1, Title = "Inception", FilmGenre = "Sci-Fi", ReleaseYear = 2010 },
                new Movie { Id = 2, Title = "The Godfather", FilmGenre = "Crime", ReleaseYear = 1972 }
            }.GetEnumerator());

            // Initialize the controller with the mocked list
            _controller = new MovieController(_mockMovies.Object);
        }

        [Fact]
        public void GetAll_ReturnsOkResult_WithListOfMovies()
        {
            // Act
            var result = _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var movies = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);
            Assert.NotEmpty(movies);
        }

        [Fact]
        public void GetById_ExistingId_ReturnsOkResult_WithMovie()
        {
            // Act
            var result = _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var movie = Assert.IsType<Movie>(okResult.Value);
            Assert.Equal(1, movie.Id);
        }

        [Fact]
        public void GetById_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_ValidMovie_ReturnsCreatedAtAction()
        {
            // Arrange
            var newMovie = new Movie { Title = "Interstellar", FilmGenre = "Sci-Fi", ReleaseYear = 2014 };

            // Act
            var result = _controller.Create(newMovie);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result.Result);
            var movie = Assert.IsType<Movie>(createdAtAction.Value);
            Assert.Equal("Interstellar", movie.Title);
        }

        [Fact]
        public void Update_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var updatedMovie = new Movie { Title = "Updated", FilmGenre = "Drama", ReleaseYear = 2020 };

            // Act
            var result = _controller.Update(1, updatedMovie);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Update_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var updatedMovie = new Movie { Title = "Updated", FilmGenre = "Drama", ReleaseYear = 2020 };

            // Act
            var result = _controller.Update(999, updatedMovie);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_ExistingId_ReturnsNoContent()
        {
            // Act
            var result = _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}