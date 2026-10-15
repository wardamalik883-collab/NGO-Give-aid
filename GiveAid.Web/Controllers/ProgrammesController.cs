using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public controller for browsing welfare programmes, submitting participation/volunteer interest,
/// and viewing personal application history.
/// Implements FR-038, FR-039, FR-040, FR-041 (Task 14).
/// </summary>
public class ProgrammesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogger<ProgrammesController> _logger;

    public ProgrammesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogger<ProgrammesController> logger)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Programmes or /Programmes/Index
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;

        var allProgrammes = await _context.Programmes
            .AsNoTracking()
            .ToListAsync();

        var upcoming = allProgrammes
            .Where(p => (p.EndDate ?? p.EventDate).Date >= today)
            .OrderBy(p => p.EventDate)
            .ToList();

        var past = allProgrammes
            .Where(p => (p.EndDate ?? p.EventDate).Date < today)
            .OrderByDescending(p => p.EventDate)
            .ToList();

        var viewModel = new ProgrammesIndexViewModel
        {
            UpcomingProgrammes = upcoming,
            PastProgrammes = past
        };

        return View(viewModel);
    }

    // =========================================================================
    // GET: /Programmes/Details/{id}
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var programme = await _context.Programmes
            .Include(p => p.GalleryImages)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id.Value);

        if (programme == null)
            return NotFound();

        var today = DateTime.UtcNow.Date;
        var isUpcoming = (programme.EndDate ?? programme.EventDate).Date >= today;

        var viewModel = new ProgrammeDetailsViewModel
        {
            Programme = programme,
            RelatedGalleryImages = programme.GalleryImages?
                .OrderByDescending(g => g.UploadedAt)
                .ToList() ?? new(),
            IsUpcoming = isUpcoming
        };

        return View(viewModel);
    }

    // =========================================================================
    // GET: /Programmes/ExpressInterest?programmeId={id} (Public Participation Form)
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> ExpressInterest(int? programmeId)
    {
        if (programmeId == null || programmeId <= 0)
        {
            return NotFound();
        }

        var programme = await _context.Programmes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == programmeId.Value);

        if (programme == null)
        {
            return NotFound();
        }

        var model = new ProgrammeInterestFormViewModel
        {
            ProgrammeId = programme.Id,
            ProgrammeTitle = programme.Title,
            ProgrammeLocation = programme.Location,
            ProgrammeDate = programme.EventDate,
            InterestType = "Volunteer"
        };

        // If user is signed in, pre-fill contact details and check for existing application
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.ApplicantName = user.FullName;
                model.ApplicantEmail = user.Email ?? string.Empty;
                model.ApplicantPhone = user.PhoneNumber ?? string.Empty;

                var existingInterest = await _context.ProgrammeInterests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(pi => pi.ProgrammeId == programme.Id &&
                        (pi.UserId == user.Id || pi.ApplicantEmail == user.Email));

                if (existingInterest != null)
                {
                    TempData["InfoMessage"] = $"You have already submitted an interest request for '{programme.Title}'. Your current request status is: {existingInterest.Status}.";
                }
            }
        }

        return View(model);
    }

    // =========================================================================
    // POST: /Programmes/ExpressInterest (Submit Participation Request)
    // =========================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExpressInterest(ProgrammeInterestFormViewModel model)
    {
        // 1. Verify programme exists
        var programme = await _context.Programmes
            .FirstOrDefaultAsync(p => p.Id == model.ProgrammeId);

        if (programme == null)
        {
            ModelState.AddModelError(nameof(model.ProgrammeId), "The selected programme does not exist.");
        }

        // 2. Validate allowed interest types
        var allowedRoles = new[] { "Volunteer", "Participant" };
        if (!allowedRoles.Contains(model.InterestType))
        {
            ModelState.AddModelError(nameof(model.InterestType), "Invalid participation role selected.");
        }

        // 3. Duplicate check for signed-in user or guest email
        string? currentUserId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        if (programme != null && !string.IsNullOrWhiteSpace(model.ApplicantEmail))
        {
            var duplicateQuery = _context.ProgrammeInterests
                .Where(pi => pi.ProgrammeId == model.ProgrammeId);

            bool isDuplicate;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                isDuplicate = await duplicateQuery.AnyAsync(pi =>
                    pi.UserId == currentUserId || pi.ApplicantEmail.ToLower() == model.ApplicantEmail.Trim().ToLower());
            }
            else
            {
                isDuplicate = await duplicateQuery.AnyAsync(pi =>
                    pi.ApplicantEmail.ToLower() == model.ApplicantEmail.Trim().ToLower());
            }

            if (isDuplicate)
            {
                ModelState.AddModelError(string.Empty,
                    $"An interest application for '{programme.Title}' with this email address has already been submitted.");
            }
        }

        if (!ModelState.IsValid)
        {
            if (programme != null)
            {
                model.ProgrammeTitle = programme.Title;
                model.ProgrammeLocation = programme.Location;
                model.ProgrammeDate = programme.EventDate;
            }
            return View(model);
        }

        // 4. Create and persist ProgrammeInterest entity
        var interest = new ProgrammeInterest
        {
            ProgrammeId = model.ProgrammeId,
            UserId = currentUserId, // bound server-side from claims
            ApplicantName = model.ApplicantName.Trim(),
            ApplicantEmail = model.ApplicantEmail.Trim(),
            ApplicantPhone = model.ApplicantPhone.Trim(),
            InterestType = model.InterestType,
            Message = model.Message?.Trim(),
            VolunteerNote = model.Message?.Trim(),
            Status = "Pending", // strictly set server-side
            SubmissionDate = DateTime.UtcNow
        };

        _context.ProgrammeInterests.Add(interest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Saved programme interest request for {ApplicantName} ({Email}) for Programme {ProgrammeId}",
            interest.ApplicantName, interest.ApplicantEmail, interest.ProgrammeId);

        // 5. Send confirmation email notification
        try
        {
            var emailSubject = $"Give-AID: Interest Request Received [{programme!.Title}]";
            var emailBody = $@"
                <h3>Thank you for your interest, {interest.ApplicantName}!</h3>
                <p>We have received your application to participate in <strong>{programme.Title}</strong> as a <strong>{interest.InterestType}</strong>.</p>
                <p><strong>Scheduled Date:</strong> {programme.EventDate:dd MMM yyyy, h:mm tt}</p>
                <p><strong>Location:</strong> {programme.Location}</p>
                <p>Your request is currently <strong>Pending</strong> review by our coordination team. We will reach out to you with route guidance and briefing notes prior to the event.</p>
                <p style='color:#666;font-size:12px;'>Give-AID Humanitarian Initiatives</p>";

            await _emailService.SendEmailAsync(interest.ApplicantEmail, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send interest confirmation email to {Email}", interest.ApplicantEmail);
        }

        TempData["SuccessMessage"] = "Your interest request has been submitted successfully. Our team will review your request.";

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(MyInterests));
        }

        return RedirectToAction(nameof(Details), new { id = model.ProgrammeId });
    }

    // =========================================================================
    // GET: /Programmes/MyInterests (Personal Application History for Signed-in User)
    // =========================================================================
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyInterests()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Challenge();
        }

        var interests = await _context.ProgrammeInterests
            .Include(pi => pi.Programme)
            .Where(pi => pi.UserId == currentUserId)
            .OrderByDescending(pi => pi.SubmissionDate)
            .AsNoTracking()
            .ToListAsync();

        var viewModel = new UserProgrammeInterestViewModel
        {
            Interests = interests,
            TotalApplications = interests.Count,
            ApprovedCount = interests.Count(pi => string.Equals(pi.Status, "Approved", StringComparison.OrdinalIgnoreCase)),
            PendingCount = interests.Count(pi => string.Equals(pi.Status, "Pending", StringComparison.OrdinalIgnoreCase)),
            RejectedCount = interests.Count(pi => string.Equals(pi.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        };

        return View(viewModel);
    }
}
