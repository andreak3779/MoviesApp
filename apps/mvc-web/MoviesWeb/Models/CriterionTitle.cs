namespace MoviesWeb.Models
{
    public class CriterionTitle : ICriterionTitle
    {
        public int MovieId { get; set; }
        public string Description { get; set; }
        public string TitleNumber { get; set; }

        public CriterionTitle()
        {
            Description = string.Empty;
            TitleNumber = string.Empty;
        }
    }
}