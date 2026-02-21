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
            // Implement authentication logic here (e.g., check password hash)
            return true; // Placeholder for successful authentication
        }
    }
}