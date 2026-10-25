using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.Services;

namespace MoviesApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class FilmGenresController : ControllerBase
{
    private readonly IFilmGenreRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public FilmGenresController(IFilmGenreRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    private string GetUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("User ID not found");

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<FilmGenre>>> GetAll()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<FilmGenre>> GetById(int id)
    {
        var filmGenre = await _repository.GetByIdAsync(id);
        if (filmGenre == null)
            return NotFound();
        return Ok(filmGenre);
    }

    [HttpPost]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<FilmGenre>> Create([FromBody] FilmGenre filmGenre)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.CreateAsync(filmGenre);

        var userId = GetUserId();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        await _auditLogService.LogAsync(userId, "CREATE", "FilmGenre", created.Id.ToString(),
            null, filmGenre, ip, ua, HttpContext.Request.Path);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] FilmGenre filmGenre)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _repository.UpdateAsync(id, filmGenre);
        if (updated == null)
            return NotFound();

        var userId = GetUserId();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        await _auditLogService.LogAsync(userId, "UPDATE", "FilmGenre", id.ToString(),
            null, filmGenre, ip, ua, HttpContext.Request.Path);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _repository.DeleteAsync(id);
        if (!result)
            return NotFound();

        var userId = GetUserId();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        await _auditLogService.LogAsync(userId, "DELETE", "FilmGenre", id.ToString(),
            null, null, ip, ua, HttpContext.Request.Path);

        return NoContent();
    }
}
