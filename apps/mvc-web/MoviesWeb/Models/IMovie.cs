namespace MoviesWeb.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A movie as returned by the Web API's <c>MovieResponseDto</c>.
    /// </summary>
    /// <remarks>
    /// The API serialises every property in camelCase (<c>id</c>,
    /// <c>releaseYear</c>, ...) and identifies movies by a GUID. Property
    /// names below mirror the wire format exactly so the default
    /// <see cref="System.Text.Json"/> deserialiser binds without attributes.
    /// </remarks>
    public interface IMovie
    {
        /// <summary>Gets or sets the movie's GUID identifier.</summary>
        Guid Id { get; set; }

        /// <summary>Gets or sets the movie title.</summary>
        string Title { get; set; }

        /// <summary>Gets or sets the plot summary / description.</summary>
        string? Description { get; set; }

        /// <summary>Gets or sets the denormalised genre name, if any.</summary>
        string? Genre { get; set; }

        /// <summary>Gets or sets the denormalised media type name, if any.</summary>
        string? MediaType { get; set; }

        /// <summary>Gets or sets the denormalised format name, if any.</summary>
        string? Format { get; set; }

        /// <summary>Gets or sets the year of release.</summary>
        int ReleaseYear { get; set; }

        /// <summary>Gets or sets a value indicating whether this is a favourite.</summary>
        bool Favourite { get; set; }

        /// <summary>Gets or sets a value indicating whether this is part of the Criterion Collection.</summary>
        bool Criterion { get; set; }

        /// <summary>Gets or sets the Criterion Collection spine number.</summary>
        int? CriterionNumber { get; set; }

        /// <summary>Gets or sets the identifier of the owning user.</summary>
        string OwnerId { get; set; }

        /// <summary>Gets or sets the creation timestamp.</summary>
        DateTime CreatedAt { get; set; }

        /// <summary>Gets or sets the last update timestamp, if the movie has been updated.</summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>Gets or sets the media types linked to this movie.</summary>
        IList<MediaTypeDto> MediaTypes { get; set; }

        /// <summary>Gets or sets the genres linked to this movie.</summary>
        IList<FilmGenreDto> FilmGenres { get; set; }

        /// <summary>Gets or sets the formats linked to this movie.</summary>
        IList<FormatDto> Formats { get; set; }
    }
}
