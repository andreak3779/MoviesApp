using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xunit;

namespace MovieCollectionApi.Tests.Controllers
{
    public class MovieControllerTest
    {
        [Fact]
        public void GetAll_ReturnsOkResult_WithListOfMovies()
        {
            // Arrange
            var controller = new MovieController();

            // Act
            var result = controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var movies = Assert.IsAssignableFrom<IEnumerable<Movie>>(okResult.Value);
            Assert.NotEmpty(movies);
        }

        [Fact]
        public void GetById_ExistingId_ReturnsOkResult_WithMovie()
        {
            // Arrange
            var controller = new MovieController();

            // Act
            var result = controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var movie = Assert.IsType<Movie>(okResult.Value);
            Assert.Equal(1, movie.Id);
        }

        [Fact]
        public void GetById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var controller = new MovieController();

            // Act
            var result = controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Create_ValidMovie_ReturnsCreatedAtAction()
        {
            // Arrange
            var controller = new MovieController();
            var newMovie = new Movie { Title = "Interstellar", Genre = "Sci-Fi", ReleaseYear = 2014 };

            // Act
            var result = controller.Create(newMovie);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result.Result);
            var movie = Assert.IsType<Movie>(createdAtAction.Value);
            Assert.Equal("Interstellar", movie.Title);
        }

        [Fact]
        public void Update_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var controller = new MovieController();
            var updatedMovie = new Movie { Title = "Updated", Genre = "Drama", ReleaseYear = 2020 };

            // Act
            var result = controller.Update(1, updatedMovie);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Update_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var controller = new MovieController();
            var updatedMovie = new Movie { Title = "Updated", Genre = "Drama", ReleaseYear = 2020 };

            // Act
            var result = controller.Update(999, updatedMovie);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Delete_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var controller = new MovieController();

            // Act
            var result = controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void Delete_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var controller = new MovieController();

            // Act
            var result = controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}