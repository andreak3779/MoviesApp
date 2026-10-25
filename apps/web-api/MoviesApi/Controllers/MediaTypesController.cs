using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.Services;

namespace MoviesApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MediaTypesController : ControllerBase
{
    private readonly IMediaTypeRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public MediaTypesController(IMediaTypeRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    private string GetUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("User ID not found");

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MediaType>>> GetAll()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MediaType>> GetById(int id)
    {
        var mediaType = await _repository.GetByIdAsync(id);
        if (mediaType == null)
            return NotFound();
        return Ok(mediaType);
    }

    [HttpPost]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<MediaType>> Create([FromBody] MediaType mediaType)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.CreateAsync(mediaType);

        var userId = GetUserId();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        await _auditLogService.LogAsync(userId, "CREATE", "MediaType", created.Id.ToString(),
            null, mediaType, ip, ua, HttpContext.Request.Path);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] MediaType mediaType)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _repository.UpdateAsync(id, mediaType);
        if (updated == null)
            return NotFound();

        var userId = GetUserId();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        await _auditLogService.LogAsync(userId, "UPDATE", "MediaType", id.ToString(),
            null, mediaType, ip, ua, HttpContext.Request.Path);

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
        await _auditLogService.LogAsync(userId, "DELETE", "MediaType", id.ToString(),
            null, null, ip, ua, HttpContext.Request.Path);

        return NoContent();
    }
}
