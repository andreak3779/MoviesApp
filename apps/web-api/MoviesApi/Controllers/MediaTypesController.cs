using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.DTOs;

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
    public async Task<ActionResult<IEnumerable<MediaTypeDto>>> GetAll()
    {
        var mediaTypes = await _repository.GetAllAsync();
        return Ok(mediaTypes.Select(MapToDto));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<MediaTypeDto>> GetById(int id)
    {
        var mediaType = await _repository.GetByIdAsync(id);
        if (mediaType == null)
            return NotFound();
        return Ok(MapToDto(mediaType));
    }

    private static MediaTypeDto MapToDto(MediaType mt) => new()
    {
        Id = mt.Id,
        Name = mt.Name,
        Description = mt.Description,
    };
}
