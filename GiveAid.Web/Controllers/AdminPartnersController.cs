using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Admin-only controller for managing Partner records (CRUD).
/// Implements FR-036: Full CRUD capabilities over Partner records.
/// Implements FR-037: Logo upload sanitization.
/// Protected by [Authorize(Roles = "Admin")] — non-admin users are redirected to AccessDenied.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Partners")]
public class AdminPartnersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ImageUploadService _imageUploadService;
    private readonly ILogger<AdminPartnersController> _logger;

    public AdminPartnersController(
        ApplicationDbContext context,
        ImageUploadService imageUploadService,
        ILogger<AdminPartnersController> logger)
    {
        _context = context;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Admin/Partners
    // =========================================================================
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var partners = await _context.Partners
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return View("~/Views/Admin/Partners/Index.cshtml", partners);
    }

    // =========================================================================
    // GET: /Admin/Partners/Details/{id}
    // =========================================================================
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var partner = await _context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (partner == null)
        {
            TempData["ErrorMessage"] = "The requested Partner record was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Partners/Details.cshtml", partner);
    }

    // =========================================================================
    // GET: /Admin/Partners/Create
    // =========================================================================
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Partners/Create.cshtml", new PartnerFormViewModel());
    }

    // =========================================================================
    // POST: /Admin/Partners/Create
    // =========================================================================
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PartnerFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Partners/Create.cshtml", model);

        var partner = new Partner
        {
            Name = model.Name,
            PartnerType = model.PartnerType,
            Description = model.Description,
            WebsiteUrl = model.WebsiteUrl,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        // Handle logo upload
        if (model.LogoFile != null && model.LogoFile.Length > 0)
        {
            var (path, error) = await _imageUploadService.SaveImageAsync(model.LogoFile, "partners");
            if (error != null)
            {
                ModelState.AddModelError("LogoFile", error);
                return View("~/Views/Admin/Partners/Create.cshtml", model);
            }
            partner.LogoPath = path;
        }

        _context.Partners.Add(partner);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Partner '{Name}' (ID: {Id}) created by {User}", partner.Name, partner.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Partner \"{partner.Name}\" has been created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Partners/Edit/{id}
    // =========================================================================
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var partner = await _context.Partners.FindAsync(id);
        if (partner == null)
        {
            TempData["ErrorMessage"] = "The requested Partner record was not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new PartnerFormViewModel
        {
            Id = partner.Id,
            Name = partner.Name,
            PartnerType = partner.PartnerType,
            Description = partner.Description,
            WebsiteUrl = partner.WebsiteUrl,
            DisplayOrder = partner.DisplayOrder,
            IsActive = partner.IsActive,
            ExistingLogoPath = partner.LogoPath
        };

        return View("~/Views/Admin/Partners/Edit.cshtml", model);
    }

    // =========================================================================
    // POST: /Admin/Partners/Edit/{id}
    // =========================================================================
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PartnerFormViewModel model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid request: ID mismatch.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Partners/Edit.cshtml", model);

        var partner = await _context.Partners.FindAsync(id);
        if (partner == null)
        {
            TempData["ErrorMessage"] = "The requested Partner record was not found.";
            return RedirectToAction(nameof(Index));
        }

        partner.Name = model.Name;
        partner.PartnerType = model.PartnerType;
        partner.Description = model.Description;
        partner.WebsiteUrl = model.WebsiteUrl;
        partner.DisplayOrder = model.DisplayOrder;
        partner.IsActive = model.IsActive;

        // Handle logo upload — preserve existing if no new file uploaded
        if (model.LogoFile != null && model.LogoFile.Length > 0)
        {
            var (path, error) = await _imageUploadService.SaveImageAsync(model.LogoFile, "partners");
            if (error != null)
            {
                ModelState.AddModelError("LogoFile", error);
                return View("~/Views/Admin/Partners/Edit.cshtml", model);
            }

            // Delete old logo if it existed
            if (!string.IsNullOrEmpty(partner.LogoPath))
                _imageUploadService.DeleteImage(partner.LogoPath);

            partner.LogoPath = path;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Partner '{Name}' (ID: {Id}) updated by {User}", partner.Name, partner.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Partner \"{partner.Name}\" has been updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Partners/Delete/{id}
    // =========================================================================
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var partner = await _context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (partner == null)
        {
            TempData["ErrorMessage"] = "The requested Partner record was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Partners/Delete.cshtml", partner);
    }

    // =========================================================================
    // POST: /Admin/Partners/Delete/{id}
    // =========================================================================
    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var partner = await _context.Partners.FindAsync(id);
        if (partner == null)
        {
            TempData["ErrorMessage"] = "The requested Partner record was not found.";
            return RedirectToAction(nameof(Index));
        }

        // Delete logo file if it exists
        if (!string.IsNullOrEmpty(partner.LogoPath))
            _imageUploadService.DeleteImage(partner.LogoPath);

        _context.Partners.Remove(partner);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Partner '{Name}' (ID: {Id}) deleted by {User}", partner.Name, partner.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Partner \"{partner.Name}\" has been deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
