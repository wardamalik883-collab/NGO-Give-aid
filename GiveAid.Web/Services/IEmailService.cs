namespace GiveAid.Web.Services;

/// <summary>
/// Service interface for dispatching notifications and query replies.
/// Implements graceful fallback when SMTP credentials are not configured.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email or safely simulates delivery in development/test environments.
    /// </summary>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="htmlMessage">HTML message body.</param>
    /// <returns>Tuple of (Success flag, descriptive status message).</returns>
    Task<(bool Success, string Message)> SendEmailAsync(string toEmail, string subject, string htmlMessage);

    /// <summary>
    /// Returns true if valid outbound SMTP server configuration is present.
    /// </summary>
    bool IsConfigured { get; }
}
