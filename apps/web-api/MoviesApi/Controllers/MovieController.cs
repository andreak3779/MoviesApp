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

        /// <summary>
        /// GET /api/v1/Movie — search / filter / sort / page the caller's
        /// movies. All query parameters are optional. The mockup-shaped
        /// filter row maps to <c>search</c>, <c>genre</c>, <c>mediaType</c>,
        /// <c>format</c>, <c>sortBy</c>.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(PagedResponse<MovieResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponse<MovieResponseDto>>> GetAll([FromQuery] MovieQuery query)
        {
            var userId = GetUserId();
            var (movies, totalCount) = await _repository.SearchAsync(userId, query);
            var response = movies.Select(MapToResponseDto).ToList();
            var paged = PagedResponse<MovieResponseDto>.Create(
                response, totalCount, query.PageNumber, query.PageSize);
            return Ok(paged);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(typeof(MovieResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MovieResponseDto>> Create([FromBody] MovieCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var movie = new Movie
            {
                Title = dto.Title,
                ReleaseYear = dto.ReleaseYear,
                Description = dto.Description,
                Favourite = dto.Favourite,
                Criterion = dto.Criterion,
                CriterionNumber = dto.Criterion ? dto.CriterionNumber : null,
                OwnerId = userId,
            };

            // Mockup uses single-select fields. Fall back to legacy list fields when the
            // single-id is not provided, so older clients keep working.
            if (dto.MediaTypeId.HasValue)
                movie.MovieMediaTypes.Add(new MovieMediaType { MediaTypeId = dto.MediaTypeId.Value });
            else
                foreach (var id in dto.MediaTypeIds)
                    movie.MovieMediaTypes.Add(new MovieMediaType { MediaTypeId = id });

            if (dto.FilmGenreId.HasValue)
                movie.MovieFilmGenres.Add(new MovieFilmGenre { FilmGenreId = dto.FilmGenreId.Value });
            else
                foreach (var id in dto.FilmGenreIds)
                    movie.MovieFilmGenres.Add(new MovieFilmGenre { FilmGenreId = id });

            if (dto.FormatId.HasValue)
                movie.MovieFormats.Add(new MovieFormat { FormatId = dto.FormatId.Value });

            var createdMovie = await _repository.CreateAsync(movie);
            if (createdMovie == null)
                return BadRequest("Failed to create movie");

            // Audit log
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
            await _auditLogService.LogAsync(userId, "CREATE", "Movie", createdMovie.Id.ToString(),
                null, dto, ipAddress, userAgent, HttpContext.Request.Path);

            var response = MapToResponseDto(createdMovie);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] MovieUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var existingMovie = await _repository.GetByIdWithOwnershipCheckAsync(id, userId);
            if (existingMovie == null)
                return NotFound();

            var oldValues = new
            {
                existingMovie.Title,
                existingMovie.ReleaseYear,
                existingMovie.Description,
                existingMovie.Favourite,
                existingMovie.Criterion,
                existingMovie.CriterionNumber,
                Genre = existingMovie.MovieFilmGenres.FirstOrDefault()?.FilmGenre.Name,
                MediaType = existingMovie.MovieMediaTypes.FirstOrDefault()?.MediaType.Name,
                Format = existingMovie.MovieFormats.FirstOrDefault()?.Format.Name,
            };

            // Build a detached desired-state document rather than mutating the
            // tracked entity. The repository diffs against its own copy, so
            // handing it the instance it returned would let the two alias and
            // wipe the link collections.
            var desired = new Movie
            {
                Id = id,
                Title = dto.Title ?? existingMovie.Title,
                ReleaseYear = dto.ReleaseYear ?? existingMovie.ReleaseYear,
                Description = dto.Description ?? existingMovie.Description,
                Favourite = dto.Favourite ?? existingMovie.Favourite,
                Criterion = dto.Criterion ?? existingMovie.Criterion,
                CriterionNumber = dto.CriterionNumber ?? existingMovie.CriterionNumber,
                OwnerId = existingMovie.OwnerId,
                CreatedAt = existingMovie.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
            };

            if (!desired.Criterion)
                desired.CriterionNumber = null;

            // Every collection carries the complete desired link set: the
            // current links when the request does not mention that collection.
            if (dto.MediaTypeId.HasValue)
            {
                desired.MovieMediaTypes = new List<MovieMediaType>
                {
                    new MovieMediaType { MediaTypeId = dto.MediaTypeId.Value },
                };
            }
            else if (dto.MediaTypeIds != null)
            {
                desired.MovieMediaTypes = dto.MediaTypeIds
                    .Select(mt => new MovieMediaType { MediaTypeId = mt })
                    .ToList();
            }
            else
            {
                desired.MovieMediaTypes = existingMovie.MovieMediaTypes
                    .Select(mm => new MovieMediaType { MediaTypeId = mm.MediaTypeId })
                    .ToList();
            }

            if (dto.FilmGenreId.HasValue)
            {
                desired.MovieFilmGenres = new List<MovieFilmGenre>
                {
                    new MovieFilmGenre { FilmGenreId = dto.FilmGenreId.Value },
                };
            }
            else if (dto.FilmGenreIds != null)
            {
                desired.MovieFilmGenres = dto.FilmGenreIds
                    .Select(fg => new MovieFilmGenre { FilmGenreId = fg })
                    .ToList();
            }
            else
            {
                desired.MovieFilmGenres = existingMovie.MovieFilmGenres
                    .Select(mfg => new MovieFilmGenre { FilmGenreId = mfg.FilmGenreId })
                    .ToList();
            }

            if (dto.FormatId.HasValue)
            {
                desired.MovieFormats = new List<MovieFormat>
                {
                    new MovieFormat { FormatId = dto.FormatId.Value },
                };
            }
            else
            {
                desired.MovieFormats = existingMovie.MovieFormats
                    .Select(mf => new MovieFormat { FormatId = mf.FormatId })
                    .ToList();
            }

            var updatedMovie = await _repository.UpdateAsync(id, desired);
            if (updatedMovie == null)
                return NotFound();

            // Audit log
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
            await _auditLogService.LogAsync(userId, "UPDATE", "Movie", id.ToString(),
                oldValues, dto, ipAddress, userAgent, HttpContext.Request.Path);

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                Description = movie.Description,
                Favourite = movie.Favourite,
                Criterion = movie.Criterion,
                CriterionNumber = movie.CriterionNumber,
                OwnerId = movie.OwnerId,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt,
                Genre = movie.MovieFilmGenres.FirstOrDefault()?.FilmGenre.Name,
                MediaType = movie.MovieMediaTypes.FirstOrDefault()?.MediaType.Name,
                Format = movie.MovieFormats.FirstOrDefault()?.Format.Name,
                MediaTypes = movie.MovieMediaTypes
                    .Select(mm => new MediaTypeDto
                    {
                        Id = mm.MediaType.Id,
                        Name = mm.MediaType.Name,
                        Description = mm.MediaType.Description,
                    }).ToList(),
                FilmGenres = movie.MovieFilmGenres
                    .Select(mf => new FilmGenreDto
                    {
                        Id = mf.FilmGenre.Id,
                        Name = mf.FilmGenre.Name,
                        Description = mf.FilmGenre.Description,
                    }).ToList(),
                Formats = movie.MovieFormats
                    .Select(mf => new FormatDto
                    {
                        Id = mf.Format.Id,
                        Name = mf.Format.Name,
                        Description = mf.Format.Description,
                    }).ToList(),
            };
        }
    }
}
