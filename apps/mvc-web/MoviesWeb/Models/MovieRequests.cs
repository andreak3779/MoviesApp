namespace MoviesWeb.Models
{
    /// <summary>
    /// The payload sent to <c>POST /api/v1/Movie</c>.
    /// </summary>
    /// <remarks>
    /// Mirrors the API's <c>MovieCreateDto</c>. The API binds the single-id
    /// fields (<c>filmGenreId</c>, <c>mediaTypeId</c>, <c>formatId</c>) and
    /// ignores the legacy list-of-ids variants when the single ids are set.
    /// </remarks>
    public class MovieCreateRequest
    {
        /// <summary>Gets or sets the movie title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the year of release.</summary>
        public int ReleaseYear { get; set; }

        /// <summary>Gets or sets the plot summary / description.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets a value indicating whether this is a favourite.</summary>
        public bool Favourite { get; set; }

        /// <summary>Gets or sets a value indicating whether this is part of the Criterion Collection.</summary>
        public bool Criterion { get; set; }

        /// <summary>Gets or sets the Criterion Collection spine number.</summary>
        public int? CriterionNumber { get; set; }

        /// <summary>Gets or sets the selected genre identifier.</summary>
        public int? FilmGenreId { get; set; }

        /// <summary>Gets or sets the selected media type identifier.</summary>
        public int? MediaTypeId { get; set; }

        /// <summary>Gets or sets the selected format identifier.</summary>
        public int? FormatId { get; set; }
    }

    /// <summary>
    /// The payload sent to <c>PUT /api/v1/Movie/{id}</c>.
    /// </summary>
    /// <remarks>
    /// Mirrors the API's <c>MovieUpdateDto</c>; every field is optional.
    /// </remarks>
    public class MovieUpdateRequest
    {
        /// <summary>Gets or sets the movie title.</summary>
        public string? Title { get; set; }

        /// <summary>Gets or sets the year of release.</summary>
        public int? ReleaseYear { get; set; }

        /// <summary>Gets or sets the plot summary / description.</summary>
        public string? Description { get; set; }

        /// <summary>Gets or sets a value indicating whether this is a favourite.</summary>
        public bool? Favourite { get; set; }

        /// <summary>Gets or sets a value indicating whether this is part of the Criterion Collection.</summary>
        public bool? Criterion { get; set; }

        /// <summary>Gets or sets the Criterion Collection spine number.</summary>
        public int? CriterionNumber { get; set; }

        /// <summary>Gets or sets the selected genre identifier.</summary>
        public int? FilmGenreId { get; set; }

        /// <summary>Gets or sets the selected media type identifier.</summary>
        public int? MediaTypeId { get; set; }

        /// <summary>Gets or sets the selected format identifier.</summary>
        public int? FormatId { get; set; }
    }
}
