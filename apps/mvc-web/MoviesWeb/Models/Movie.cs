namespace MoviesWeb.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A movie as returned by the Web API's <c>MovieResponseDto</c>.
    /// </summary>
    /// <remarks>
    /// The API serialises every property in camelCase and identifies movies by
    /// a GUID, so the property names here mirror the wire format exactly.
    /// </remarks>
    public class Movie : IMovie
    {
        /// <inheritdoc />
        public Guid Id { get; set; }

        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public string? Description { get; set; }

        /// <inheritdoc />
        public string? Genre { get; set; }

        /// <inheritdoc />
        public string? MediaType { get; set; }

        /// <inheritdoc />
        public string? Format { get; set; }

        /// <inheritdoc />
        public int ReleaseYear { get; set; }

        /// <inheritdoc />
        public bool Favourite { get; set; }

        /// <inheritdoc />
        public bool Criterion { get; set; }

        /// <inheritdoc />
        public int? CriterionNumber { get; set; }

        /// <inheritdoc />
        public string OwnerId { get; set; }

        /// <inheritdoc />
        public DateTime CreatedAt { get; set; }

        /// <inheritdoc />
        public DateTime? UpdatedAt { get; set; }

        /// <inheritdoc />
        public IList<MediaTypeDto> MediaTypes { get; set; } = new List<MediaTypeDto>();

        /// <inheritdoc />
        public IList<FilmGenreDto> FilmGenres { get; set; } = new List<FilmGenreDto>();

        /// <inheritdoc />
        public IList<FormatDto> Formats { get; set; } = new List<FormatDto>();

        public Movie()
        {
            Title = string.Empty;
            OwnerId = string.Empty;
        }
    }
}