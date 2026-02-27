    namespace MoviesWeb.ViewModels;
    
    /// <summary>
    /// Represents the data required to render a list of movies, including paging metadata.
    /// </summary>
    public class MovieListModel
    {
        /// <summary>
        /// Gets or sets the list of movies to display on the current page.
        /// </summary>
        public List<Movie> Movies { get; set; } = new();
    
        /// <summary>
        /// Gets or sets the current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }
    
        /// <summary>
        /// Gets or sets the size of each page (number of items per page).
        /// </summary>
        public int PageSize { get; set; }
    
        /// <summary>
        /// Gets or sets the total number of items available across all pages.
        /// </summary>
        public int TotalCount { get; set; }
    
        /// <summary>
        /// Gets a value indicating whether there is a previous page of results.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    
        /// <summary>
        /// Gets a value indicating whether there is a next page of results.
        /// </summary>
        public bool HasNextPage => PageSize > 0 && (long)PageNumber * PageSize < TotalCount;
    }
