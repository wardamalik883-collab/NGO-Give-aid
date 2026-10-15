using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Handles user-initiated simulated donations, demo payment execution, personal receipts, and donor history.
/// STRICT SECURITY: Authentication is required to donate or view records. Real card data is NEVER collected.
/// </summary>
[Authorize]
public class DonateController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISimulatedPaymentService _paymentService;
    private readonly ILogger<DonateController> _logger;

    public DonateController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ISimulatedPaymentService paymentService,
        ILogger<DonateController> logger)
    {
        _context = context;
        _userManager = userManager;
        _paymentService = paymentService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Donate or /Donate/Index?causeId={id}
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Index(int? causeId = null)
    {
        var model = new DonationFormViewModel();

        // 1. Load active causes for dropdown selection
        await PopulateAvailableCausesAsync(model);

        // 2. Pre-select cause if specified in query string
        if (causeId.HasValue && causeId.Value > 0)
        {
            var cause = await _context.Causes
                .Include(c => c.Ngo)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == causeId.Value && c.IsActive);

            if (cause != null)
            {
                model.CauseId = cause.Id;
                model.SelectedCauseTitle = cause.Title;
                model.SelectedCauseNgoName = cause.Ngo?.Name ?? "Give-AID Partner NGO";
                model.SelectedCauseTargetAmount = cause.TargetAmount;
                model.SelectedCauseCurrentAmount = cause.CurrentAmount;
            }
        }

        // 3. Pre-populate donor profile information from authenticated user context
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            model.DonorName = string.IsNullOrWhiteSpace(model.DonorName) ? user.FullName : model.DonorName;
            model.DonorEmail = string.IsNullOrWhiteSpace(model.DonorEmail) ? (user.Email ?? string.Empty) : model.DonorEmail;
            model.DonorPhone = string.IsNullOrWhiteSpace(model.DonorPhone) ? user.PhoneNumber : model.DonorPhone;
        }

        return View(model);
    }

    // =========================================================================
    // POST: /Donate or /Donate/Index (Form Submission -> Proceed to Payment Step)
    // =========================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DonationFormViewModel model)
    {
        // 1. Server-side validation: Ensure Cause exists and is active
        Cause? targetCause = null;
        if (model.CauseId.HasValue && model.CauseId.Value > 0)
        {
            targetCause = await _context.Causes
                .Include(c => c.Ngo)
                .FirstOrDefaultAsync(c => c.Id == model.CauseId.Value && c.IsActive);

            if (targetCause == null)
            {
                ModelState.AddModelError(nameof(model.CauseId), "The selected charitable cause does not exist or is currently inactive.");
            }
        }
        else
        {
            ModelState.AddModelError(nameof(model.CauseId), "Please select an active cause to support.");
        }

        // 2. Server-side validation: Amount bounds
        if (!model.Amount.HasValue || model.Amount <= 0)
        {
            ModelState.AddModelError(nameof(model.Amount), "Donation amount must be greater than zero.");
        }
        else if (model.Amount > 10000000m)
        {
            ModelState.AddModelError(nameof(model.Amount), "Donation amount cannot exceed PKR 10,000,000 per transaction.");
        }

        // If validation fails, repopulate causes dropdown and return form with errors
        if (!ModelState.IsValid)
        {
            await PopulateAvailableCausesAsync(model);

            if (targetCause != null)
            {
                model.SelectedCauseTitle = targetCause.Title;
                model.SelectedCauseNgoName = targetCause.Ngo?.Name ?? "Give-AID Partner NGO";
                model.SelectedCauseTargetAmount = targetCause.TargetAmount;
                model.SelectedCauseCurrentAmount = targetCause.CurrentAmount;
            }

            return View(model);
        }

        // 3. Validation passed: Build simulated payment step model
        var paymentModel = new SimulatedPaymentViewModel
        {
            CauseId = targetCause!.Id,
            CauseTitle = targetCause.Title,
            NgoName = targetCause.Ngo?.Name ?? "Give-AID Partner NGO",
            Amount = model.Amount!.Value,
            DonorName = model.DonorName.Trim(),
            DonorEmail = model.DonorEmail.Trim(),
            DonorPhone = string.IsNullOrWhiteSpace(model.DonorPhone) ? null : model.DonorPhone.Trim(),
            SimulatedOutcome = "Success", // default deterministic demo outcome
            IdempotencyToken = Guid.NewGuid().ToString("N")
        };

        return View("Payment", paymentModel);
    }

    // =========================================================================
    // POST: /Donate/ProcessPayment (Simulated Demo Payment Execution)
    // =========================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(SimulatedPaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Payment", model);
        }

        // Securely retrieve the authenticated user's ID from claims — never trust browser parameters
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var request = new SimulatedPaymentRequest
        {
            CauseId = model.CauseId,
            Amount = model.Amount,
            UserId = currentUserId,
            DonorName = model.DonorName,
            DonorEmail = model.DonorEmail,
            DonorPhone = model.DonorPhone,
            RequestedOutcome = model.SimulatedOutcome,
            SimulatedNotes = model.SimulatedNotes,
            IdempotencyToken = model.IdempotencyToken
        };

        var result = await _paymentService.ProcessSimulatedPaymentAsync(request);

        if (result.DonationId > 0)
        {
            return RedirectToAction(nameof(Receipt), new { id = result.DonationId });
        }

        ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Payment processing encountered an unexpected error.");
        return View("Payment", model);
    }

    // =========================================================================
    // GET: /Donate/Receipt/{id} (Donation Confirmation & Receipt)
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
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

        // Ownership enforcement: Only the owner or an administrator can view this receipt
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isAdmin = User.IsInRole("Admin");

        if (!isAdmin && donation.UserId != currentUserId)
        {
            _logger.LogWarning("Access Denied: User {UserId} attempted to view receipt {DonationId} owned by {OwnerId}",
                currentUserId, id, donation.UserId);
            return Forbid();
        }

        var viewModel = new DonationReceiptViewModel
        {
            DonationId = donation.Id,
            TransactionReference = donation.TransactionReference,
            Amount = donation.Amount,
            DonationDate = donation.DonationDate,
            PaymentStatus = donation.PaymentStatus,
            IsSimulated = donation.IsSimulated,
            CauseId = donation.CauseId,
            CauseTitle = donation.Cause?.Title ?? "General Relief Cause",
            NgoName = donation.Cause?.Ngo?.Name ?? "Give-AID Partner NGO",
            DonorName = donation.DonorName,
            DonorEmail = donation.DonorEmail,
            DonorPhone = donation.DonorPhone,
            PaymentNotes = donation.PaymentNotes,
            IsAdminView = isAdmin
        };

        return View(viewModel);
    }

    // =========================================================================
    // GET: /Donate/History (Authenticated User Personal Donation Ledger)
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Challenge();
        }

        var donations = await _context.Donations
            .Include(d => d.Cause)
                .ThenInclude(c => c!.Ngo)
            .Where(d => d.UserId == currentUserId)
            .OrderByDescending(d => d.DonationDate)
            .AsNoTracking()
            .ToListAsync();

        var completedDonations = donations
            .Where(d => string.Equals(d.PaymentStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(d.PaymentStatus, "Successful", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var viewModel = new UserDonationHistoryViewModel
        {
            Donations = donations,
            TotalContributed = completedDonations.Sum(d => d.Amount),
            TotalDonationsCount = donations.Count,
            SuccessfulDonationsCount = completedDonations.Count,
            UniqueCausesSupportedCount = completedDonations.Select(d => d.CauseId).Distinct().Count()
        };

        return View(viewModel);
    }

    // =========================================================================
    // Helpers
    // =========================================================================
    private async Task PopulateAvailableCausesAsync(DonationFormViewModel model)
    {
        var causes = await _context.Causes
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title,
                Selected = model.CauseId.HasValue && model.CauseId.Value == c.Id
            })
            .ToListAsync();

        model.AvailableCauses = causes;
    }
}
