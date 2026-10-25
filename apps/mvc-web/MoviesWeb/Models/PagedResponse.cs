namespace MoviesWeb.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The paged envelope returned by <c>GET /api/v1/Movie</c>.
    /// </summary>
    /// <remarks>
    /// Mirrors the API's <c>PagedResponse&lt;T&gt;</c>. Property names match
    /// the camelCase wire format.
    /// </remarks>
    /// <typeparam name="T">The type of item in the page.</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>Gets or sets the items on the current page.</summary>
        public List<T> Items { get; set; } = new();

        /// <summary>Gets or sets the current page number (1-based).</summary>
        public int PageNumber { get; set; }

        /// <summary>Gets or sets the number of items per page.</summary>
        public int PageSize { get; set; }

        /// <summary>Gets or sets the total number of matching items.</summary>
        public int TotalCount { get; set; }

        /// <summary>Gets or sets the total number of pages.</summary>
        public int TotalPages { get; set; }

        /// <summary>Gets or sets a value indicating whether a previous page exists.</summary>
        public bool HasPrevious { get; set; }

        /// <summary>Gets or sets a value indicating whether a next page exists.</summary>
        public bool HasNext { get; set; }
    }
}
