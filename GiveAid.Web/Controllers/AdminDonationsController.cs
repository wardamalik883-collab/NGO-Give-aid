using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Administrator controller for monitoring and auditing all platform donation records.
/// Protected exclusively for users in the "Admin" role.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Donations")]
public class AdminDonationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminDonationsController> _logger;

    public AdminDonationsController(
        ApplicationDbContext context,
        ILogger<AdminDonationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Admin/Donations
    // =========================================================================
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? status = null, int? causeId = null, string? search = null)
    {
        var allDonations = await _context.Donations
            .Include(d => d.Cause)
                .ThenInclude(c => c!.Ngo)
            .Include(d => d.User)
            .OrderByDescending(d => d.DonationDate)
            .AsNoTracking()
            .ToListAsync();

        // Calculate KPI metrics across all donations
        var successfulDonations = allDonations
            .Where(d => string.Equals(d.PaymentStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(d.PaymentStatus, "Successful", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var totalRaised = successfulDonations.Sum(d => d.Amount);
        var totalCount = allDonations.Count;
        var successCount = successfulDonations.Count;
        var failedCount = allDonations.Count(d => string.Equals(d.PaymentStatus, "Failed", StringComparison.OrdinalIgnoreCase));

        // Apply filters to query list
        var filtered = allDonations.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (string.Equals(status, "Success", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(d => string.Equals(d.PaymentStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(d.PaymentStatus, "Successful", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                filtered = filtered.Where(d => string.Equals(d.PaymentStatus, status, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (causeId.HasValue && causeId.Value > 0)
        {
            filtered = filtered.Where(d => d.CauseId == causeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim();
            filtered = filtered.Where(d =>
                d.TransactionReference.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.DonorName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.DonorEmail.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        // Available causes for filter dropdown
        var causesList = await _context.Causes
            .OrderBy(c => c.Title)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title,
                Selected = causeId.HasValue && causeId.Value == c.Id
            })
            .ToListAsync();

        var viewModel = new AdminDonationListViewModel
        {
            Donations = filtered.ToList(),
            StatusFilter = status,
            CauseFilter = causeId,
            SearchQuery = search,
            TotalAmountRaised = totalRaised,
            TotalCount = totalCount,
            SuccessfulCount = successCount,
            FailedCount = failedCount,
            CausesList = causesList
        };

        return View("~/Views/Admin/Donations/Index.cshtml", viewModel);
    }

    // =========================================================================
    // GET: /Admin/Donations/Details/{id}
    // =========================================================================
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var donation = await _context.Donations
            .Include(d => d.Cause)
                .ThenInclude(c => c!.Ngo)
            .Include(d => d.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/Donations/Details.cshtml", donation);
    }

    // =========================================================================
    // POST: /Admin/Donations/UpdateStatus
    // =========================================================================
    [HttpPost("UpdateStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string newStatus, string? adminNotes)
    {
        var donation = await _context.Donations
            .Include(d => d.Cause)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (donation == null)
        {
            return NotFound();
        }

        var validStatuses = new[] { "Completed", "Failed", "Pending" };
        if (!validStatuses.Contains(newStatus))
        {
            TempData["ErrorMessage"] = $"Invalid status '{newStatus}' specified.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var oldStatus = donation.PaymentStatus;
        bool wasSuccessful = string.Equals(oldStatus, "Completed", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(oldStatus, "Successful", StringComparison.OrdinalIgnoreCase);
        bool willBeSuccessful = string.Equals(newStatus, "Completed", StringComparison.OrdinalIgnoreCase);

        // Adjust cause progress accordingly if status changes
        if (donation.Cause != null)
        {
            if (wasSuccessful && !willBeSuccessful)
            {
                donation.Cause.CurrentAmount = Math.Max(0m, donation.Cause.CurrentAmount - donation.Amount);
            }
            else if (!wasSuccessful && willBeSuccessful)
            {
                donation.Cause.CurrentAmount += donation.Amount;
            }
        }

        donation.PaymentStatus = newStatus;
        if (!string.IsNullOrWhiteSpace(adminNotes))
        {
            donation.PaymentNotes = string.IsNullOrWhiteSpace(donation.PaymentNotes)
                ? $"[Admin Audit]: {adminNotes.Trim()}"
                : $"{donation.PaymentNotes} | [Admin Audit]: {adminNotes.Trim()}";
        }
        donation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin updated donation {Reference} status from {OldStatus} to {NewStatus}",
            donation.TransactionReference, oldStatus, newStatus);

        TempData["SuccessMessage"] = $"Donation {donation.TransactionReference} status was updated to '{newStatus}'.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
