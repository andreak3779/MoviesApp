namespace MoviesWeb.Models
{
    using System;

    public interface IMovie
    {
        int Id { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        string Genre { get; set; }
        string Director { get; set; }
        int ReleaseYear { get; set; }
        DateTime? ReleaseDate { get; set; }
        ICriterionTitle? CriterionTitle { get; set; }
    }
}
