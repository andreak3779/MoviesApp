namespace MoviesWeb.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using MoviesWeb.Models;
    using MoviesWeb.Services;

    /// <summary>
    /// Handles signing in and out against the Web API.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IApiAuthService _authService;
        private readonly IMovieSession _movieSession;
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="authService">Service used to talk to the API's auth endpoints.</param>
        /// <param name="movieSession">Session store for the issued tokens.</param>
        /// <param name="logger">Logger for sign-in failures.</param>
        public AccountController(
            IApiAuthService authService,
            IMovieSession movieSession,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _movieSession = movieSession;
            _logger = logger;
        }

        /// <summary>
        /// Shows the sign-in form.
        /// </summary>
        /// <param name="returnUrl">Optional URL to return to after signing in.</param>
        /// <returns>The login view.</returns>
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginRequest());
        }

        /// <summary>
        /// Signs the user in by exchanging their credentials for API tokens.
        /// </summary>
        /// <param name="request">The submitted credentials.</param>
        /// <param name="returnUrl">Optional URL to return to after signing in.</param>
        /// <returns>A redirect on success; the login view with an error otherwise.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(request);
            }

            AuthResponse? auth;
            try
            {
                auth = await _authService.LoginAsync(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Sign-in request to the Web API failed.");
                ModelState.AddModelError(string.Empty, "Could not reach the Movies API. Please try again.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(request);
            }

            if (auth is null)
            {
                ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(request);
            }

            // The API tokens live in the session; the auth cookie only records
            // that the browser is signed in.
            _movieSession.SignIn(auth);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, auth.Email),
                new(ClaimTypes.Email, auth.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Movies");
        }

        /// <summary>
        /// Signs the user out and clears the stored tokens.
        /// </summary>
        /// <returns>A redirect to the login page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _movieSession.SignOut();
            return RedirectToAction("Login");
        }
    }
}
