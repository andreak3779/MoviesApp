namespace MyApp.Namespace
{
    public class MoviesController : Controller
    {
        private readonly IMoviesService _moviesService;
        public MoviesController(IMoviesService moviesService) => _moviesService = moviesService;

        // GET: MovieController
        public async Task<ActionResult> Index()
        {
            var movies = await _moviesService.GetMoviesAsync();
            if (movies == null)
            {
                return NotFound();
            }
            return View("Index", movies);
        }
        
        // GET: MovieController/Details/5
        public async Task<ActionResult> MovieDetails(int id)
        {
            var movie = await _moviesService.GetMovieAsync(id);
            if (movie == null || movie.Id == 0)
            {
                return NotFound();
            }
            return View("MovieDetails", movie);
        }
    }
}
