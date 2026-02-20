namespace MoviesWeb.Models
{
    using System.Collections.Generic;

    public interface IUser
    {
        int UserId { get; set; }
        string Name { get; set; }
        string Email { get; set; }
        IList<IUserFavouriteMovies>? UserFavouriteMovies { get; set; }

        bool Authenticate(string password);
    }
}
