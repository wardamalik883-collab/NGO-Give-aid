using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Controller for authenticated members to invite friends and spread awareness.
/// Implements Task 15 & Task 18 requirements.
/// </summary>
[Authorize]
public class InviteController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<InviteController> _logger;

    public InviteController(ApplicationDbContext context, IEmailService emailService, ILogger<InviteController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        ViewBag.RecentInvitations = await _context.Invitations
            .Where(i => i.SenderUserId == currentUserId)
            .OrderByDescending(i => i.SentAt)
            .Take(5)
            .AsNoTracking()
            .ToListAsync();

        return View(new InvitationFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(InvitationFormViewModel model)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        if (!ModelState.IsValid)
        {
            ViewBag.RecentInvitations = await _context.Invitations
                .Where(i => i.SenderUserId == currentUserId)
                .OrderByDescending(i => i.SentAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            return View(model);
        }

        var senderName = User.Identity?.Name ?? "A community supporter";

        var invitation = new Invitation
        {
            SenderUserId = currentUserId,
            RecipientEmail = model.RecipientEmail.Trim().ToLowerInvariant(),
            InviteMessage = model.Message?.Trim(),
            SentAt = DateTime.UtcNow,
            DeliveryStatus = _emailService.IsConfigured ? "Sent" : "Simulated"
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync();

        var subject = $"Give-AID NGO: Invitation from {senderName}";
        var body = $@"
            <h3>Hello!</h3>
            <p>Your friend <strong>{senderName}</strong> has invited you to visit the <strong>Give-AID Humanitarian NGO</strong> community platform.</p>
            {(string.IsNullOrWhiteSpace(model.Message) ? "" : $"<p><strong>Personal Message:</strong> {model.Message}</p>")}
            <p>Visit our website to discover active welfare campaigns, community relief drives, and emergency welfare causes.</p>
            <p><a href='{Request.Scheme}://{Request.Host}/'>Visit Give-AID Platform &rarr;</a></p>
            <hr />
            <p style='color:#666;font-size:12px;'>Give-AID Humanitarian Initiatives</p>";

        var emailResult = await _emailService.SendEmailAsync(invitation.RecipientEmail, subject, body);

        if (_emailService.IsConfigured)
        {
            if (emailResult.Success)
            {
                TempData["SuccessMessage"] = $"Your invitation has been dispatched to {invitation.RecipientEmail}.";
            }
            else
            {
                invitation.DeliveryStatus = "Failed";
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = $"Invitation recorded, but email dispatch failed: {emailResult.Message}";
            }
        }
        else
        {
            TempData["SuccessMessage"] = $"Invitation recorded successfully! (Email delivery was safely simulated in demo mode because SMTP is unconfigured).";
        }

        return RedirectToAction(nameof(Index));
    }
}
