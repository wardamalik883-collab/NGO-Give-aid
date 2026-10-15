using System.Net;
using System.Net.Mail;

namespace GiveAid.Web.Services;

/// <summary>
/// Configurable SMTP email service.
/// Supports standard environment variables and user secrets for credentials.
/// Safely logs and simulates delivery when credentials are not configured,
/// ensuring query management operations are never blocked.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public bool IsConfigured
    {
        get
        {
            var host = _configuration["EmailSettings:Host"] ?? _configuration["EmailSettings:SmtpServer"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            return !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(senderEmail);
        }
    }

    public async Task<(bool Success, string Message)> SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return (false, "Recipient email address cannot be empty.");
        }

        if (!IsConfigured)
        {
            _logger.LogInformation(
                "[SIMULATED EMAIL] To: {ToEmail} | Subject: {Subject} | Delivery simulated because SMTP settings are not configured.",
                toEmail, subject);

            return (true, "Email notification simulated (SMTP credentials not configured).");
        }

        try
        {
            var host = _configuration["EmailSettings:Host"] ?? _configuration["EmailSettings:SmtpServer"]!;
            var portString = _configuration["EmailSettings:Port"] ?? "587";
            var port = int.TryParse(portString, out var p) ? p : 587;
            var senderEmail = _configuration["EmailSettings:SenderEmail"]!;
            var senderName = _configuration["EmailSettings:SenderName"] ?? "Give-AID NGO Support";
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var enableSsl = bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) ? ssl : true;

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            await client.SendMailAsync(message);
            _logger.LogInformation("Email successfully dispatched to {ToEmail} with subject '{Subject}'.", toEmail, subject);
            return (true, $"Email dispatched successfully to {toEmail}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'.", toEmail, subject);
            return (false, $"Email delivery failed: {ex.Message}");
        }
    }
}
