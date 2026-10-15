using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Administrator controller for reviewing, approving, and rejecting programme participation and volunteer interest requests.
/// Protected exclusively for users in the "Admin" role.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Interests")]
public class AdminInterestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminInterestsController> _logger;

    public AdminInterestsController(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<AdminInterestsController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Admin/Interests
    // =========================================================================
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? status = null, int? programmeId = null, string? search = null)
    {
        var allInterests = await _context.ProgrammeInterests
            .Include(pi => pi.Programme)
            .Include(pi => pi.User)
            .OrderByDescending(pi => pi.SubmissionDate)
            .AsNoTracking()
            .ToListAsync();

        var totalCount = allInterests.Count;
        var pendingCount = allInterests.Count(pi => string.Equals(pi.Status, "Pending", StringComparison.OrdinalIgnoreCase)
                                                || string.Equals(pi.Status, "Submitted", StringComparison.OrdinalIgnoreCase));
        var approvedCount = allInterests.Count(pi => string.Equals(pi.Status, "Approved", StringComparison.OrdinalIgnoreCase));
        var rejectedCount = allInterests.Count(pi => string.Equals(pi.Status, "Rejected", StringComparison.OrdinalIgnoreCase));

        var filtered = allInterests.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(pi => string.Equals(pi.Status, "Pending", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(pi.Status, "Submitted", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                filtered = filtered.Where(pi => string.Equals(pi.Status, status, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (programmeId.HasValue && programmeId.Value > 0)
        {
            filtered = filtered.Where(pi => pi.ProgrammeId == programmeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filtered = filtered.Where(pi =>
                pi.ApplicantName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                pi.ApplicantEmail.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                pi.ApplicantPhone.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var programmesList = await _context.Programmes
            .OrderBy(p => p.Title)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Title,
                Selected = programmeId.HasValue && programmeId.Value == p.Id
            })
            .ToListAsync();

        var viewModel = new AdminInterestListViewModel
        {
            Interests = filtered.ToList(),
            StatusFilter = status,
            ProgrammeFilter = programmeId,
            SearchQuery = search,
            TotalCount = totalCount,
            PendingCount = pendingCount,
            ApprovedCount = approvedCount,
            RejectedCount = rejectedCount,
            ProgrammesList = programmesList
        };

        return View("~/Views/Admin/Interests/Index.cshtml", viewModel);
    }

    // =========================================================================
    // GET: /Admin/Interests/Details/{id}
    // =========================================================================
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var interest = await _context.ProgrammeInterests
            .Include(pi => pi.Programme)
            .Include(pi => pi.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(pi => pi.Id == id);

        if (interest == null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/Interests/Details.cshtml", interest);
    }

    // =========================================================================
    // POST: /Admin/Interests/Approve/{id}
    // =========================================================================
    [HttpPost("Approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? adminNotes = null)
    {
        var interest = await _context.ProgrammeInterests
            .Include(pi => pi.Programme)
            .FirstOrDefaultAsync(pi => pi.Id == id);

        if (interest == null)
        {
            return NotFound();
        }

        interest.Status = "Approved";
        interest.ReviewedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(adminNotes))
        {
            interest.AdminNotes = string.IsNullOrWhiteSpace(interest.AdminNotes)
                ? $"[Approved]: {adminNotes.Trim()}"
                : $"{interest.AdminNotes} | [Approved]: {adminNotes.Trim()}";
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin approved interest application {Id} from {ApplicantName} for Programme {ProgrammeId}",
            interest.Id, interest.ApplicantName, interest.ProgrammeId);

        // Notify applicant via email
        try
        {
            var emailSubject = $"Give-AID: Application Approved! [{interest.Programme?.Title}]";
            var emailBody = $@"
                <h3>Congratulations, {interest.ApplicantName}!</h3>
                <p>Your application to join <strong>{interest.Programme?.Title}</strong> as a <strong>{interest.InterestType}</strong> has been <strong>Approved</strong>.</p>
                <p><strong>Scheduled Date:</strong> {interest.Programme?.EventDate:dd MMM yyyy, h:mm tt}</p>
                <p><strong>Location:</strong> {interest.Programme?.Location}</p>
                <p>Our volunteer coordination lead will reach out with final logistical instructions shortly.</p>
                <p style='color:#666;font-size:12px;'>Give-AID Humanitarian Initiatives</p>";

            await _emailService.SendEmailAsync(interest.ApplicantEmail, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send approval email to {Email}", interest.ApplicantEmail);
        }

        TempData["SuccessMessage"] = $"Interest request from {interest.ApplicantName} has been approved successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // =========================================================================
    // POST: /Admin/Interests/Reject/{id}
    // =========================================================================
    [HttpPost("Reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminNotes = null)
    {
        var interest = await _context.ProgrammeInterests
            .Include(pi => pi.Programme)
            .FirstOrDefaultAsync(pi => pi.Id == id);

        if (interest == null)
        {
            return NotFound();
        }

        interest.Status = "Rejected";
        interest.ReviewedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(adminNotes))
        {
            interest.AdminNotes = string.IsNullOrWhiteSpace(interest.AdminNotes)
                ? $"[Rejected]: {adminNotes.Trim()}"
                : $"{interest.AdminNotes} | [Rejected]: {adminNotes.Trim()}";
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin rejected interest application {Id} from {ApplicantName} for Programme {ProgrammeId}",
            interest.Id, interest.ApplicantName, interest.ProgrammeId);

        // Notify applicant via email
        try
        {
            var emailSubject = $"Give-AID: Application Update [{interest.Programme?.Title}]";
            var emailBody = $@"
                <h3>Hello {interest.ApplicantName},</h3>
                <p>Thank you for expressing interest in <strong>{interest.Programme?.Title}</strong>.</p>
                <p>Due to volunteer capacity limits for this specific drive, we are unable to accommodate your application at this time.</p>
                <p>We encourage you to explore our other upcoming community initiatives and welfare drives.</p>
                <p style='color:#666;font-size:12px;'>Give-AID Humanitarian Initiatives</p>";

            await _emailService.SendEmailAsync(interest.ApplicantEmail, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send status update email to {Email}", interest.ApplicantEmail);
        }

        TempData["SuccessMessage"] = $"Interest request from {interest.ApplicantName} has been marked as Rejected.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
