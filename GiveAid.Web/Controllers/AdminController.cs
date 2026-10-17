using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Controller for administrative overview and core dashboard.
/// Access is strictly protected by the "Admin" role.
/// Non-admin users are automatically redirected to AccessDenied.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly ApplicationDbContext _context;

    public AdminController(ILogger<AdminController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Admin Dashboard landing and analytics page.
    /// Gathers live summary metrics and recent activity records across all modules.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Admin dashboard accessed by {User}", User.Identity?.Name);

        var model = new AdminDashboardViewModel
        {
            TotalCauses = await _context.Causes.CountAsync(),
            ActiveCausesCount = await _context.Causes.CountAsync(c => c.IsActive),
            TotalDonations = await _context.Donations.CountAsync(),
            TotalDonationsAmount = await _context.Donations
                .Where(d => d.PaymentStatus == "Completed")
                .SumAsync(d => (decimal?)d.Amount) ?? 0m,
            TotalMembers = await _context.Users.CountAsync(),
            TotalNGOs = await _context.NGOs.CountAsync(),
            TotalPartners = await _context.Partners.CountAsync(),
            TotalProgrammes = await _context.Programmes.CountAsync(),
            ActiveProgrammesCount = await _context.Programmes.CountAsync(p => p.Status == "Scheduled"),
            TotalGalleryItems = await _context.GalleryImages.CountAsync(),
            TotalContactQueries = await _context.ContactQueries.CountAsync(),
            PendingContactQueries = await _context.ContactQueries.CountAsync(q => q.Status == "New"),
            PendingProgrammeRequests = await _context.ProgrammeInterests.CountAsync(pi => pi.Status == "Pending"),
            TotalInvitations = await _context.Invitations.CountAsync(),

            RecentDonations = await _context.Donations
                .Include(d => d.Cause)
                .AsNoTracking()
                .OrderByDescending(d => d.DonationDate)
                .Take(5)
                .Select(d => new RecentDonationItemViewModel
                {
                    Id = d.Id,
                    TransactionReference = d.TransactionReference,
                    Amount = d.Amount,
                    DonorName = d.DonorName,
                    PaymentStatus = d.PaymentStatus,
                    CauseTitle = d.Cause != null ? d.Cause.Title : "General Cause",
                    DonationDate = d.DonationDate
                })
                .ToListAsync(),

            RecentQueries = await _context.ContactQueries
                .AsNoTracking()
                .OrderByDescending(q => q.SubmittedAt)
                .Take(5)
                .Select(q => new RecentQueryItemViewModel
                {
                    Id = q.Id,
                    SenderName = q.SenderName,
                    Subject = q.Subject,
                    Status = q.Status,
                    SubmittedAt = q.SubmittedAt
                })
                .ToListAsync(),

            RecentInterests = await _context.ProgrammeInterests
                .Include(pi => pi.Programme)
                .AsNoTracking()
                .OrderByDescending(pi => pi.SubmissionDate)
                .Take(5)
                .Select(pi => new RecentInterestItemViewModel
                {
                    Id = pi.Id,
                    ApplicantName = pi.ApplicantName,
                    ProgrammeTitle = pi.Programme != null ? pi.Programme.Title : "Welfare Drive",
                    InterestType = pi.InterestType,
                    Status = pi.Status,
                    SubmissionDate = pi.SubmissionDate
                })
                .ToListAsync()
        };

        return View(model);
    }
}
