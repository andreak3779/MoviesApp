namespace MyApp.Namespace
{
    public class MoviesController : Controller
    {
        public MoviesController(IMovieServices movieServices)
        {
            
        }
        
        // GET: MovieController
        public ActionResult Index()
        {
            return View();
        }

    }
}
