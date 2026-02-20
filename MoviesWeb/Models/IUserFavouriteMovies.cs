namespace MoviesWeb.Models
{
    using System.Collections.Generic;

    public interface IUserFavouriteMovies
    {
        int UserFavouriteMoviesId { get; set; }
        int UserId { get; set; }
        IUser? User { get; set; }
        IList<IMovie>? Movies { get; set; }
    }
}
