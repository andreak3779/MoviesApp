using Microsoft.AspNetCore.Mvc;
using MoviesApi.Models;
using MoviesApi.Repositories;

namespace MoviesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaTypesController : ControllerBase
{
    private readonly IMediaTypeRepository _repository;

    public MediaTypesController(IMediaTypeRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MediaType>>> GetAll()
    {
        var mediaTypes = await _repository.GetAllAsync();
        return Ok(mediaTypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MediaType>> GetById(int id)
    {
        var mediaType = await _repository.GetByIdAsync(id);
        if (mediaType == null)
            return NotFound();
        return Ok(mediaType);
    }
}
