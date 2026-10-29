namespace MoviesWeb.ViewModels
{
    /// <summary>
    /// The model behind the error page.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>Gets or sets the identifier of the failed request.</summary>
        public string? RequestId { get; set; }

        /// <summary>Gets a value indicating whether a request identifier is available.</summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
