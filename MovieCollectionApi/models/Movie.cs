using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

public class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string FilmGenre { get; set; }
    public int ReleaseYear { get; set; }
}