namespace MoviesWeb.Models
{
    using System;

    public class Movie : IMovie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }
        public int ReleaseYear { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public virtual ICriterionTitle? CriterionTitle { get; set; }

        public Movie()
        {
            Title = string.Empty;
            Description = string.Empty;
            Genre = string.Empty;
            Director = string.Empty;
        }
    }
}