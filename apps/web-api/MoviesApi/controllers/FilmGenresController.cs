using Microsoft.AspNetCore.Mvc;
using MoviesApi.Models;
using MoviesApi.Repositories;

namespace MoviesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilmGenresController : ControllerBase
{
    private readonly IFilmGenreRepository _repository;

    public FilmGenresController(IFilmGenreRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FilmGenre>>> GetAll()
    {
        var filmGenres = await _repository.GetAllAsync();
        return Ok(filmGenres);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FilmGenre>> GetById(int id)
    {
        var filmGenre = await _repository.GetByIdAsync(id);
        if (filmGenre == null)
            return NotFound();
        return Ok(filmGenre);
    }
}
