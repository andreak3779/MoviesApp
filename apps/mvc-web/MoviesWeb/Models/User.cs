namespace MoviesWeb.Models
{
    using System.Collections.Generic;

    public class User : IUser
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public virtual IList<IUserFavouriteMovies>? UserFavouriteMovies { get; set; }

        public User()
        {
            Name = string.Empty;
            Email = string.Empty;
            UserFavouriteMovies = null;
        }

        public bool Authenticate(string password)
        {
            // Authentication is intentionally not implemented in this demo
            // project. A previous version of this method returned `true`
            // unconditionally, which made any caller appear authenticated.
            // See docs/SECURITY-OVERVIEW.md.
            throw new NotImplementedException(
                "Authentication is not implemented. Wire this up through ASP.NET " +
                "Core Identity (see MoviesApi) before relying on this path.");
        }
    }
}