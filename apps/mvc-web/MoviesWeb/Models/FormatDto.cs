namespace MoviesWeb.Models
{
    /// <summary>
    /// A format as returned by the Web API's <c>FormatDto</c>.
    /// </summary>
    public class FormatDto
    {
        /// <summary>Gets or sets the format identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the format name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the format description.</summary>
        public string? Description { get; set; }
    }
}
