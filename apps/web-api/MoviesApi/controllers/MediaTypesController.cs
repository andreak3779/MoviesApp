using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Models;
using MoviesApi.Repositories;

namespace MoviesApi.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class MediaTypesController : ControllerBase
{
    private readonly IMediaTypeRepository _repository;

    public MediaTypesController(IMediaTypeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<MediaType>>> GetAll()
    {
        var mediaTypes = await _repository.GetAllAsync();
        return Ok(mediaTypes);
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
}
