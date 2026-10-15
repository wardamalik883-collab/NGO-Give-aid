using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Administrator controller for managing visitor inquiries and contact queries.
/// Implements FR-052: Admin inquiry dashboard with status filtering and audit.
/// Implements FR-053: Admin query status tracking, internal notes, and official replies.
/// Protected by [Authorize(Roles = "Admin")].
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Queries")]
public class AdminQueriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminQueriesController> _logger;

    private static readonly string[] ValidStatuses = { "New", "In Progress", "Answered", "Closed" };

    public AdminQueriesController(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<AdminQueriesController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Admin/Queries
    // =========================================================================
    [HttpGet("")]
    public async Task<IActionResult> Index(string? status)
    {
        var allQueries = await _context.ContactQueries
            .Include(q => q.User)
            .OrderByDescending(q => q.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();

        var viewModel = new AdminQueryListViewModel
        {
            TotalCount = allQueries.Count,
            NewCount = allQueries.Count(q => q.Status.Equals("New", StringComparison.OrdinalIgnoreCase)),
            InProgressCount = allQueries.Count(q => q.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase)),
            AnsweredCount = allQueries.Count(q => q.Status.Equals("Answered", StringComparison.OrdinalIgnoreCase)),
            ClosedCount = allQueries.Count(q => q.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase)),
            SelectedStatus = status
        };

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            viewModel.Queries = allQueries
                .Where(q => q.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        else
        {
            viewModel.Queries = allQueries;
        }

        return View("~/Views/Admin/Queries/Index.cshtml", viewModel);
    }

    // =========================================================================
    // GET: /Admin/Queries/Details/{id}
    // =========================================================================
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var query = await _context.ContactQueries
            .Include(q => q.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);

        if (query == null)
        {
            TempData["ErrorMessage"] = "The requested inquiry was not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new AdminQueryReplyViewModel
        {
            QueryId = query.Id,
            SenderName = query.SenderName,
            SenderEmail = query.SenderEmail,
            SenderPhone = query.SenderPhone,
            Subject = query.Subject,
            OriginalMessage = query.Message,
            SubmittedAt = query.SubmittedAt,
            CurrentStatus = query.Status,
            AdminNotes = query.AdminNotes,
            ExistingReply = query.AdminReply,
            AdminReply = query.AdminReply ?? string.Empty,
            RepliedAt = query.RepliedAt,
            UserId = query.UserId,
            RegisteredUserEmail = query.User?.Email
        };

        return View("~/Views/Admin/Queries/Details.cshtml", viewModel);
    }

    // =========================================================================
    // POST: /Admin/Queries/Reply/{id}
    // =========================================================================
    [HttpPost("Reply/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, AdminQueryReplyViewModel model)
    {
        if (id != model.QueryId)
        {
            TempData["ErrorMessage"] = "Invalid request: Query ID mismatch.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(model.AdminReply))
        {
            ModelState.AddModelError("AdminReply", "Please provide a non-empty reply message.");
        }

        if (!ModelState.IsValid)
        {
            // Re-populate non-submitted display fields
            var existing = await _context.ContactQueries.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
            if (existing != null)
            {
                model.SenderName = existing.SenderName;
                model.SenderEmail = existing.SenderEmail;
                model.SenderPhone = existing.SenderPhone;
                model.Subject = existing.Subject;
                model.OriginalMessage = existing.Message;
                model.SubmittedAt = existing.SubmittedAt;
                model.CurrentStatus = existing.Status;
                model.ExistingReply = existing.AdminReply;
                model.RepliedAt = existing.RepliedAt;
            }
            return View("~/Views/Admin/Queries/Details.cshtml", model);
        }

        var query = await _context.ContactQueries.FindAsync(id);
        if (query == null)
        {
            TempData["ErrorMessage"] = "The requested inquiry was not found.";
            return RedirectToAction(nameof(Index));
        }

        query.AdminReply = model.AdminReply.Trim();
        query.RepliedAt = DateTime.UtcNow;
        query.Status = "Answered";

        if (!string.IsNullOrWhiteSpace(model.AdminNotes))
        {
            query.AdminNotes = model.AdminNotes.Trim();
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Administrator {Admin} recorded official reply for query #{Id}.", User.Identity?.Name, id);

        string emailFeedback = string.Empty;
        if (model.SendEmailNotification)
        {
            var emailSubject = $"Re: {query.Subject} — Give-AID Support Response";
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #0d6efd;'>Give-AID Humanitarian Response</h2>
                    <p>Dear <strong>{System.Net.WebUtility.HtmlEncode(query.SenderName)}</strong>,</p>
                    <p>Thank you for reaching out to Give-AID regarding <em>'{System.Net.WebUtility.HtmlEncode(query.Subject)}'</em>.</p>
                    <hr style='border: 0; border-top: 1px solid #eee;' />
                    <div style='background: #f8f9fa; padding: 15px; border-left: 4px solid #0d6efd; margin: 15px 0;'>
                        <p style='margin: 0; white-space: pre-line;'>{System.Net.WebUtility.HtmlEncode(query.AdminReply)}</p>
                    </div>
                    <p style='color: #6c757d; font-size: 13px;'>If you have additional questions, feel free to reply to this email or visit our Help Centre.</p>
                    <p style='margin-bottom: 0;'>Warm regards,<br /><strong>Give-AID Operations Team</strong></p>
                </div>";

            var (success, msg) = await _emailService.SendEmailAsync(query.SenderEmail, emailSubject, emailBody);
            emailFeedback = success ? $" [Notification dispatched: {msg}]" : $" [Email delivery notice: {msg}]";
        }

        TempData["SuccessMessage"] = $"Reply saved for query #{id}. Status marked as 'Answered'.{emailFeedback}";
        return RedirectToAction(nameof(Details), new { id });
    }

    // =========================================================================
    // POST: /Admin/Queries/UpdateStatus/{id}
    // =========================================================================
    [HttpPost("UpdateStatus/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? adminNotes)
    {
        var query = await _context.ContactQueries.FindAsync(id);
        if (query == null)
        {
            TempData["ErrorMessage"] = "The requested inquiry was not found.";
            return RedirectToAction(nameof(Index));
        }

        var matchedStatus = ValidStatuses.FirstOrDefault(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
        if (matchedStatus == null)
        {
            TempData["ErrorMessage"] = $"Invalid status '{status}'. Allowed statuses: {string.Join(", ", ValidStatuses)}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        query.Status = matchedStatus;
        if (adminNotes != null)
        {
            query.AdminNotes = string.IsNullOrWhiteSpace(adminNotes) ? null : adminNotes.Trim();
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Status of query #{Id} updated to '{Status}' by {Admin}.", id, matchedStatus, User.Identity?.Name);

        TempData["SuccessMessage"] = $"Status of query #{id} updated to '{matchedStatus}'.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // =========================================================================
    // GET: /Admin/Queries/Delete/{id}
    // =========================================================================
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var query = await _context.ContactQueries
            .Include(q => q.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);

        if (query == null)
        {
            TempData["ErrorMessage"] = "The requested inquiry was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Queries/Delete.cshtml", query);
    }

    // =========================================================================
    // POST: /Admin/Queries/Delete/{id}
    // =========================================================================
    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var query = await _context.ContactQueries.FindAsync(id);
        if (query == null)
        {
            TempData["ErrorMessage"] = "The requested inquiry was not found.";
            return RedirectToAction(nameof(Index));
        }

        _context.ContactQueries.Remove(query);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Contact query #{Id} was deleted by {Admin}.", id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Inquiry #{id} from '{query.SenderName}' was permanently removed.";
        return RedirectToAction(nameof(Index));
    }
}
