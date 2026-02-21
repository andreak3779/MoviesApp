namespace MoviesWeb.Models
{
    public interface ICriterionTitle
    {
        int MovieId { get; set; }
        string Description { get; set; }
        string TitleNumber { get; set; }
    }
}
