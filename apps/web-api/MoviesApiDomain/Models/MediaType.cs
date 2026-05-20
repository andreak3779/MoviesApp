using System.ComponentModel.DataAnnotations;

namespace MoviesApiDomain.Models;

/// <summary>
/// Represents a media format type (e.g., DVD, Blu-Ray, 4K Ultra HD, Digital)
/// </summary>
public class MediaType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    // Navigation property for many-to-many relationship
    public ICollection<MovieMediaType> MovieMediaTypes { get; set; } = new List<MovieMediaType>();
}
