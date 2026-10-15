using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public controller for the Contact Us inquiry submission workflow.
/// Implements FR-050: Public Contact Us inquiry form.
/// Implements FR-051: Server-side validation for contact submissions.
/// Implements FR-054: Contact queries linked to logged-in user profile.
/// Accessible to both anonymous visitors and authenticated members.
/// </summary>
[Route("Contact")]
public class ContactController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ContactController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Contact
    // =========================================================================
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var model = new ContactFormViewModel();

        if (User.Identity?.IsAuthenticated == true)
        {
            model.IsAuthenticated = true;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                model.Name = user.FullName;
                model.Email = user.Email ?? string.Empty;
                model.Phone = user.PhoneNumber;
            }
        }

        return View(model);
    }

    // =========================================================================
    // POST: /Contact
    // =========================================================================
    [HttpPost("")]
    [HttpPost("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactFormViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            model.IsAuthenticated = true;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var query = new ContactQuery
            {
                SenderName = model.Name.Trim(),
                SenderEmail = model.Email.Trim(),
                SenderPhone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                Subject = model.Subject.Trim(),
                Message = model.Message.Trim(),
                SubmittedAt = DateTime.UtcNow,
                Status = "New"
            };

            // Associate with logged-in user profile safely via server-side claims (FR-054)
            if (User.Identity?.IsAuthenticated == true)
            {
                query.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            _context.ContactQueries.Add(query);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "New inquiry received from '{Sender}' ({Email}). Subject: '{Subject}'. Query ID: {Id}",
                query.SenderName, query.SenderEmail, query.Subject, query.Id);

            TempData["SuccessMessage"] = "Thank you! Your inquiry has been submitted successfully. Our team will review your message and respond shortly.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist contact inquiry for '{Email}'.", model.Email);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving your inquiry. Please try again or reach out to our emergency helpline.");
            return View(model);
        }
    }
}
