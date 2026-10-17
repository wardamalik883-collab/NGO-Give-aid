namespace GiveAid.Web.Models;

/// <summary>
/// Enhanced error view model for friendly status code and diagnostic reporting.
/// Implements Task 17: Validation, Security and Error Handling.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int? StatusCode { get; set; }

    public string Title { get; set; } = "An Error Occurred";

    public string Message { get; set; } = "An unexpected error occurred while processing your request. Please try again or return to the home page.";

    public string ActionUrl { get; set; } = "/";

    public string ActionText { get; set; } = "Return to Home";
}
