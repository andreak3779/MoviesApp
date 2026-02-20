using MoviesWeb.Models;

namespace MoviesWeb.Models
{
    public class UserFavouriteMovies
    {
        public int UserFavouriteMoviesId { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
        public virtual List<Movie>? Movies { get; set; }
    }
}
