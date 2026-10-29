namespace MoviesWeb.Controllers
{
    using System.Diagnostics;
    using Microsoft.AspNetCore.Mvc;
    using MoviesWeb.ViewModels;

    /// <summary>
    /// Serves the static pages: the home page, the privacy notice and the error page.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Shows the home page.
        /// </summary>
        /// <returns>The home view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Shows the privacy notice.
        /// </summary>
        /// <returns>The privacy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Shows the error page.
        /// </summary>
        /// <returns>The error view.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
