
namespace MyApp.Namespace
{
    public class MovieController : Controller
    {

        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        }
        // GET: MovieController
        public IActionResult Index()
        {
            return View("List");
        }

    }
}
