using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.DTOs;
using MoviesApi.Services;

namespace MoviesApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class MovieController : ControllerBase
    {
        private readonly IMovieRepository _repository;
        private readonly ILogger<MovieController> _logger;
        private readonly IAuditLogService _auditLogService;

        public MovieController(IMovieRepository repository, ILogger<MovieController> logger, IAuditLogService auditLogService)
        {
            _repository = repository;
            _logger = logger;
            _auditLogService = auditLogService;
        }

        private string GetUserId() => 
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("User ID not found");

        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<PagedResponse<MovieResponseDto>>> GetAll([FromQuery] PaginationParameters pagination)
        {
            var userId = GetUserId();
            var (movies, totalCount) = await _repository.GetPagedByUserAsync(userId, pagination.PageNumber, pagination.PageSize);
            var response = movies.Select(MapToResponseDto).ToList();
            var pagedResponse = PagedResponse<MovieResponseDto>.Create(response, totalCount, pagination.PageNumber, pagination.PageSize);
            return Ok(pagedResponse);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<MovieResponseDto>> GetById(Guid id)
        {
            var userId = GetUserId();
            var movie = await _repository.GetByIdWithOwnershipCheckAsync(id, userId);
            if (movie == null)
                return NotFound();
            return Ok(MapToResponseDto(movie));
        }

        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        public async Task<ActionResult<MovieResponseDto>> Create([FromBody] MovieCreateDto movieDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var movie = new Movie
            {
                Title = movieDto.Title,
                ReleaseYear = movieDto.ReleaseYear,
                OwnerId = userId
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

            // Audit log
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
            await _auditLogService.LogAsync(userId, "CREATE", "Movie", createdMovie.Id.ToString(),
                null, Services.SensitiveFieldRedactor.Redact(movieDto), ipAddress, userAgent, HttpContext.Request.Path);

            var response = MapToResponseDto(createdMovie);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MovieUpdateDto movieDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var existingMovie = await _repository.GetByIdWithOwnershipCheckAsync(id, userId);
            if (existingMovie == null)
                return NotFound();

            // Capture old values before update
            var oldValues = new
            {
                existingMovie.Title,
                existingMovie.ReleaseYear,
                MediaTypeIds = existingMovie.MovieMediaTypes.Select(mm => mm.MediaTypeId).ToList(),
                FilmGenreIds = existingMovie.MovieFilmGenres.Select(mf => mf.FilmGenreId).ToList()
            };

            // Update properties
            if (movieDto.Title != null)
                existingMovie.Title = movieDto.Title;
            if (movieDto.ReleaseYear.HasValue)
                existingMovie.ReleaseYear = movieDto.ReleaseYear.Value;
            
            existingMovie.UpdatedAt = DateTime.UtcNow;

            // Update many-to-many relationships if provided
            if (movieDto.MediaTypeIds != null)
            {
                existingMovie.MovieMediaTypes.Clear();
                foreach (var mediaTypeId in movieDto.MediaTypeIds)
                {
                    existingMovie.MovieMediaTypes.Add(new MovieMediaType { MediaTypeId = mediaTypeId });
                }
            }

            if (movieDto.FilmGenreIds != null)
            {
                existingMovie.MovieFilmGenres.Clear();
                foreach (var filmGenreId in movieDto.FilmGenreIds)
                {
                    existingMovie.MovieFilmGenres.Add(new MovieFilmGenre { FilmGenreId = filmGenreId });
                }
            }

            var updatedMovie = await _repository.UpdateAsync(id, existingMovie);
            if (updatedMovie == null)
                return NotFound();

            // Audit log
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
            await _auditLogService.LogAsync(userId, "UPDATE", "Movie", id.ToString(),
                oldValues, Services.SensitiveFieldRedactor.Redact(movieDto), ipAddress, userAgent, HttpContext.Request.Path);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "UserOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            var result = await _repository.DeleteWithOwnershipCheckAsync(id, userId);
            if (!result)
                return NotFound();

            // Audit log
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
            await _auditLogService.LogAsync(userId, "DELETE", "Movie", id.ToString(), 
                null, null, ipAddress, userAgent, HttpContext.Request.Path);

            return NoContent();
        }

        private static MovieResponseDto MapToResponseDto(Movie movie)
        {
            return new MovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                ReleaseYear = movie.ReleaseYear,
                OwnerId = movie.OwnerId,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt,
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