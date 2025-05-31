using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace MovieCollectionApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        // In-memory list for demonstration purposes
        private static readonly List<Movie> Movies = new List<Movie>
        {
            new Movie { Id = 1, Title = "Inception", Genre = "Sci-Fi", ReleaseYear = 2010 },
            new Movie { Id = 2, Title = "The Godfather", Genre = "Crime", ReleaseYear = 1972 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Movie>> GetAll()
        {
            return Ok(Movies);
        }

        [HttpGet("{id}")]
        public ActionResult<Movie> GetById(int id)
        {
            var movie = Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();
            return Ok(movie);
        }

        [HttpPost]
        public ActionResult<Movie> Create(Movie movie)
        {
            movie.Id = Movies.Count > 0 ? Movies.Max(m => m.Id) + 1 : 1;
            Movies.Add(movie);
            return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Movie updatedMovie)
        {
            var movie = Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            movie.Title = updatedMovie.Title;
            movie.Genre = updatedMovie.Genre;
            movie.ReleaseYear = updatedMovie.ReleaseYear;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            Movies.Remove(movie);
            return NoContent();
        }
    }
}