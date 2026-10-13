using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Controller handling public donation initiation and form validation.
/// STRICT SCOPE (Task 09): UI and ModelState validation only.
/// Simulated payment transactions and receipts are handled in Task 13.
/// Real credit card numbers or credentials are NEVER collected or stored.
/// </summary>
public class DonateController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DonateController> _logger;

    public DonateController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DonateController> logger)
    {
        _context = context;
        _userManager = userManager;
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

        // 2. Pre-select cause if parameter passed
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

        // 3. Pre-populate donor info if user is authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.DonorName = user.FullName;
                model.DonorEmail = user.Email ?? string.Empty;
                model.DonorPhone = user.PhoneNumber;
            }
        }

        return View(model);
    }

    // =========================================================================
    // POST: /Donate or /Donate/Index
    // =========================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DonationFormViewModel model)
    {
        // Server-side validation: Ensure Cause exists and is active
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

        // Server-side validation: Amount bounds
        if (!model.Amount.HasValue || model.Amount <= 0)
        {
            ModelState.AddModelError(nameof(model.Amount), "Donation amount must be greater than zero.");
        }
        else if (model.Amount > 10000000m)
        {
            ModelState.AddModelError(nameof(model.Amount), "Donation amount cannot exceed PKR 10,000,000 per transaction.");
        }

        // If validation fails, repopulate causes dropdown and return view preserving inputs
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

        // Validation passed! Build review/confirmation model (Task 09 scope: Validation Complete UI)
        _logger.LogInformation("Donation form validated for Cause {CauseId} by {DonorName} for amount {Amount}",
            model.CauseId, model.DonorName, model.Amount);

        var confirmationModel = new DonationConfirmationViewModel
        {
            CauseId = targetCause!.Id,
            CauseTitle = targetCause.Title,
            NgoName = targetCause.Ngo?.Name ?? "Give-AID Partner NGO",
            Amount = model.Amount!.Value,
            DonorName = model.DonorName.Trim(),
            DonorEmail = model.DonorEmail.Trim(),
            DonorPhone = string.IsNullOrWhiteSpace(model.DonorPhone) ? null : model.DonorPhone.Trim(),
            ValidatedAtUtc = DateTime.UtcNow
        };

        return View("Confirmation", confirmationModel);
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
