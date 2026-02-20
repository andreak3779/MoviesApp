namespace MoviesWeb.Models
{
    using System.Collections.Generic;

    public class UserFavouriteMovies : IUserFavouriteMovies
    {
        public int UserFavouriteMoviesId { get; set; }
        public int UserId { get; set; }

        public virtual IUser? User { get; set; }
        public virtual IList<IMovie>? Movies { get; set; }
    }
}
