namespace MoviesWeb.Models{
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public virtual List<UserFavouriteMovies>? UserFavouriteMovies { get; set; }
    public bool Authenticate(string password)
    {
        // Implement authentication logic here (e.g., check password hash)
        return true; // Placeholder for successful authentication
    }
    
}
}