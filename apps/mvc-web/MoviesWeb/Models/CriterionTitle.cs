namespace MoviesWeb.Models
{
    using System;

    /// <summary>
    /// A Criterion Collection entry, mirroring the API's
    /// <c>Criterion</c> / <c>CriterionNumber</c> fields on a movie.
    /// </summary>
    public class CriterionTitle : ICriterionTitle
    {
        /// <inheritdoc />
        public Guid MovieId { get; set; }

        /// <inheritdoc />
        public int? TitleNumber { get; set; }

        /// <inheritdoc />
        public string? Description { get; set; }
    }
}
