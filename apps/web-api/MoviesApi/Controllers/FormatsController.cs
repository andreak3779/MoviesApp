using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.Services;

namespace MoviesApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class FormatsController : ControllerBase
{
    private readonly IFormatRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public FormatsController(IFormatRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    private string GetUserId() =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("User ID not found");

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Format>>> GetAll()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Format>> GetById(int id)
    {
        var format = await _repository.GetByIdAsync(id);
        if (format == null)
            return NotFound();
        return Ok(format);
    }

    [HttpPost]
    [Authorize(Policy = "UserOnly")]
    public async Task<ActionResult<Format>> Create([FromBody] Format format)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.CreateAsync(format);

        var userId = GetUserId();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        await _auditLogService.LogAsync(userId, "CREATE", "Format", created.Id.ToString(),
            null, format, ip, ua, HttpContext.Request.Path);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "UserOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] Format format)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _repository.UpdateAsync(id, format);
        if (updated == null)
            return NotFound();

        var userId = GetUserId();
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = HttpContext.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        await _auditLogService.LogAsync(userId, "UPDATE", "Format", id.ToString(),
            null, format, ip, ua, HttpContext.Request.Path);

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
        await _auditLogService.LogAsync(userId, "DELETE", "Format", id.ToString(),
            null, null, ip, ua, HttpContext.Request.Path);

        return NoContent();
    }
}
