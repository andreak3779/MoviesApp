namespace MoviesWeb.Models
{
    using System;

    /// <summary>
    /// The payload accepted by <c>POST /api/v1/auth/login</c>.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>Gets or sets the account email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the account password.</summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// The payload accepted by <c>POST /api/v1/auth/register</c>.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>Gets or sets the account email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the account password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the password confirmation.</summary>
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>Gets or sets the given name.</summary>
        public string? FirstName { get; set; }

        /// <summary>Gets or sets the family name.</summary>
        public string? LastName { get; set; }
    }

    /// <summary>
    /// The response returned by the Web API's auth endpoints.
    /// </summary>
    public class AuthResponse
    {
        /// <summary>Gets or sets the short-lived JWT access token.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Gets or sets the refresh token used to mint new access tokens.</summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>Gets or sets the access token expiry timestamp.</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>Gets or sets the authenticated user's email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the authenticated user's given name.</summary>
        public string? FirstName { get; set; }

        /// <summary>Gets or sets the authenticated user's family name.</summary>
        public string? LastName { get; set; }
    }

    /// <summary>
    /// The payload accepted by <c>POST /api/v1/auth/refresh</c>.
    /// </summary>
    public class RefreshRequest
    {
        /// <summary>Gets or sets the (expired) access token.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Gets or sets the refresh token.</summary>
        public string RefreshToken { get; set; } = string.Empty;
    }
}
