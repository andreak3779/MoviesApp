namespace MoviesWeb.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using MoviesWeb.Models;
    using MoviesWeb.Services;

    /// <summary>
    /// Handles signing in, creating an account and signing out against the Web API.
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
            if (!ModelState.IsValid)
            {
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

            return await SignInAsync(auth, returnUrl);
        }

        /// <summary>
        /// Shows the create-account form.
        /// </summary>
        /// <returns>The register view.</returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterRequest());
        }

        /// <summary>
        /// Creates an account and signs the new user in.
        /// </summary>
        /// <param name="request">The submitted registration.</param>
        /// <returns>A redirect on success; the register view with an error otherwise.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            AuthResponse? auth;
            try
            {
                auth = await _authService.RegisterAsync(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Registration request to the Web API failed.");
                ModelState.AddModelError(string.Empty, "Could not reach the Movies API. Please try again.");
                return View(request);
            }

            if (auth is null)
            {
                // The API answers with the same message whether the email is
                // taken or the password was rejected, so the form does not
                // tell the user which it was.
                ModelState.AddModelError(string.Empty, "Registration failed.");
                return View(request);
            }

            return await SignInAsync(auth, returnUrl: null);
        }

        /// <summary>
        /// Signs the user out and clears the stored tokens.
        /// </summary>
        /// <returns>A redirect to the login page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Revoke the refresh token server-side first: a token that stays
            // valid on the API can still be used to mint a new session.
            var refreshToken = _movieSession.RefreshToken;
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                try
                {
                    await _authService.LogoutAsync(_movieSession.AccessToken ?? string.Empty, refreshToken);
                }
                catch (HttpRequestException ex)
                {
                    // The local session is ended either way; a failed revoke
                    // must not keep the user signed in.
                    _logger.LogWarning(ex, "Could not revoke the refresh token.");
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _movieSession.SignOut();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Stores the API tokens in the session and issues the auth cookie.
        /// </summary>
        /// <param name="auth">The tokens the API issued.</param>
        /// <param name="returnUrl">Optional URL to return to after signing in.</param>
        /// <returns>A redirect to the movies list, or back to where the user came from.</returns>
        private async Task<IActionResult> SignInAsync(AuthResponse auth, string? returnUrl)
        {
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
    }
}
