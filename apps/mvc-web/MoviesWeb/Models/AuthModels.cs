namespace MoviesWeb.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The payload accepted by <c>POST /api/v1/auth/login</c>.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>Gets or sets the account email address.</summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the account password.</summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// The payload accepted by <c>POST /api/v1/auth/register</c>.
    /// </summary>
    /// <remarks>
    /// The validation rules mirror the API's <c>RegisterDto</c>, so the form
    /// can tell the user what is wrong before the request is sent.
    /// </remarks>
    public class RegisterRequest
    {
        /// <summary>Gets or sets the account email address.</summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [MaxLength(256, ErrorMessage = "Email is too long.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the account password.</summary>
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(128, ErrorMessage = "Password is too long.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter and a number.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the password confirmation.</summary>
        [Required(ErrorMessage = "Confirm the password.")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>Gets or sets the given name.</summary>
        [MaxLength(100, ErrorMessage = "First name is too long.")]
        public string? FirstName { get; set; }

        /// <summary>Gets or sets the family name.</summary>
        [MaxLength(100, ErrorMessage = "Last name is too long.")]
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
