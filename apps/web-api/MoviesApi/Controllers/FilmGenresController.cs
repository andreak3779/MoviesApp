using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;
using MoviesApi.DTOs;

namespace MoviesApi.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class FilmGenresController : ControllerBase
{
    private readonly IFilmGenreRepository _repository;

    public FilmGenresController(IFilmGenreRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<FilmGenreDto>>> GetAll()
    {
        var filmGenres = await _repository.GetAllAsync();
        return Ok(filmGenres.Select(MapToDto));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<FilmGenreDto>> GetById(int id)
    {
        var filmGenre = await _repository.GetByIdAsync(id);
        if (filmGenre == null)
            return NotFound();
        return Ok(MapToDto(filmGenre));
    }

    private static FilmGenreDto MapToDto(FilmGenre fg) => new()
    {
        Id = fg.Id,
        Name = fg.Name,
        Description = fg.Description,
    };
}
