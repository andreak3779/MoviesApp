namespace MoviesWeb.Models
{
    /// <summary>
    /// A media type as returned by the Web API's <c>MediaTypeDto</c>.
    /// </summary>
    public class MediaTypeDto
    {
        /// <summary>Gets or sets the media type identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the media type name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the media type description.</summary>
        public string? Description { get; set; }
    }
}
