using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoviesApi.Models;
using MoviesApi.Repositories;
using MoviesApi.DTOs;

namespace MoviesApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieRepository _repository;

        public MovieController(IMovieRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MovieResponseDto>>> GetAll()
        {
            var movies = await _repository.GetAllAsync();
            var response = movies.Select(MapToResponseDto);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MovieResponseDto>> GetById(int id)
        {
            var movie = await _repository.GetByIdAsync(id);
            if (movie == null)
                return NotFound();
            return Ok(MapToResponseDto(movie));
        }

        [HttpPost]
        public async Task<ActionResult<MovieResponseDto>> Create([FromBody] MovieDto movieDto)
        {
            var movie = new Movie
            {
                Title = movieDto.Title,
                ReleaseYear = movieDto.ReleaseYear
            };

            // Add many-to-many relationships
            foreach (var mediaTypeId in movieDto.MediaTypeIds)
            {
                movie.MovieMediaTypes.Add(new MovieMediaType { MediaTypeId = mediaTypeId });
            }

            foreach (var filmGenreId in movieDto.FilmGenreIds)
            {
                movie.MovieFilmGenres.Add(new MovieFilmGenre { FilmGenreId = filmGenreId });
            }

            var createdMovie = await _repository.CreateAsync(movie);
            if (createdMovie == null)
            {
                return BadRequest("Failed to create movie");
            }
            var response = MapToResponseDto(createdMovie);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MovieDto movieDto)
        {
            var movie = new Movie
            {
                Title = movieDto.Title,
                ReleaseYear = movieDto.ReleaseYear
            };

            // Add many-to-many relationships
            foreach (var mediaTypeId in movieDto.MediaTypeIds)
            {
                movie.MovieMediaTypes.Add(new MovieMediaType { MediaTypeId = mediaTypeId });
            }

            foreach (var filmGenreId in movieDto.FilmGenreIds)
            {
                movie.MovieFilmGenres.Add(new MovieFilmGenre { FilmGenreId = filmGenreId });
            }

            var updatedMovie = await _repository.UpdateAsync(id, movie);
            if (updatedMovie == null)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        private static MovieResponseDto MapToResponseDto(Movie movie)
        {
            return new MovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                ReleaseYear = movie.ReleaseYear,
                MediaTypes = movie.MovieMediaTypes
                    .Select(mm => new MediaTypeDto
                    {
                        Id = mm.MediaType.Id,
                        Name = mm.MediaType.Name,
                        Description = mm.MediaType.Description
                    }).ToList(),
                FilmGenres = movie.MovieFilmGenres
                    .Select(mf => new FilmGenreDto
                    {
                        Id = mf.FilmGenre.Id,
                        Name = mf.FilmGenre.Name,
                        Description = mf.FilmGenre.Description
                    }).ToList()
            };
        }
    }
}