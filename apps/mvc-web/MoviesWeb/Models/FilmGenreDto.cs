namespace MoviesWeb.Models
{
    /// <summary>
    /// A film genre as returned by the Web API's <c>FilmGenreDto</c>.
    /// </summary>
    public class FilmGenreDto
    {
        /// <summary>Gets or sets the genre identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the genre name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the genre description.</summary>
        public string? Description { get; set; }
    }
}
