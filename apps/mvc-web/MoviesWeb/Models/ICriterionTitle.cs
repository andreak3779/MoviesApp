namespace MoviesWeb.Models
{
    using System;

    /// <summary>
    /// A Criterion Collection entry, mirroring the API's
    /// <c>Criterion</c> / <c>CriterionNumber</c> fields on a movie.
    /// </summary>
    public interface ICriterionTitle
    {
        /// <summary>Gets or sets the GUID of the movie this entry belongs to.</summary>
        Guid MovieId { get; set; }

        /// <summary>Gets or sets the Criterion Collection spine number.</summary>
        int? TitleNumber { get; set; }

        /// <summary>Gets or sets the description of the entry.</summary>
        string? Description { get; set; }
    }
}
