using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesApiDomain.Models;
using MoviesApiData.Repositories;

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
    public async Task<ActionResult<IEnumerable<FilmGenre>>> GetAll()
    {
        var filmGenres = await _repository.GetAllAsync();
        return Ok(filmGenres);
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
}
