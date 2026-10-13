using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Admin-only controller for managing NGO records (CRUD).
/// Implements FR-035: Full CRUD capabilities over NGO records.
/// Implements FR-037: Logo upload sanitization.
/// Protected by [Authorize(Roles = "Admin")] — non-admin users are redirected to AccessDenied.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Ngos")]
public class AdminNgosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ImageUploadService _imageUploadService;
    private readonly ILogger<AdminNgosController> _logger;

    public AdminNgosController(
        ApplicationDbContext context,
        ImageUploadService imageUploadService,
        ILogger<AdminNgosController> logger)
    {
        _context = context;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Admin/Ngos
    // =========================================================================
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var ngos = await _context.NGOs
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return View("~/Views/Admin/Ngos/Index.cshtml", ngos);
    }

    // =========================================================================
    // GET: /Admin/Ngos/Details/{id}
    // =========================================================================
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var ngo = await _context.NGOs
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (ngo == null)
        {
            TempData["ErrorMessage"] = "The requested NGO record was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Ngos/Details.cshtml", ngo);
    }

    // =========================================================================
    // GET: /Admin/Ngos/Create
    // =========================================================================
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Ngos/Create.cshtml", new NgoFormViewModel());
    }

    // =========================================================================
    // POST: /Admin/Ngos/Create
    // =========================================================================
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NgoFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Ngos/Create.cshtml", model);

        var ngo = new NGO
        {
            Name = model.Name,
            RegistrationNumber = model.RegistrationNumber,
            Description = model.Description,
            Address = model.Address,
            Email = model.Email,
            Phone = model.Phone,
            WebsiteUrl = model.WebsiteUrl,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        // Handle logo upload
        if (model.LogoFile != null && model.LogoFile.Length > 0)
        {
            var (path, error) = await _imageUploadService.SaveImageAsync(model.LogoFile, "ngos");
            if (error != null)
            {
                ModelState.AddModelError("LogoFile", error);
                return View("~/Views/Admin/Ngos/Create.cshtml", model);
            }
            ngo.LogoPath = path;
        }

        _context.NGOs.Add(ngo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("NGO '{Name}' (ID: {Id}) created by {User}", ngo.Name, ngo.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"NGO \"{ngo.Name}\" has been created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Ngos/Edit/{id}
    // =========================================================================
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var ngo = await _context.NGOs.FindAsync(id);
        if (ngo == null)
        {
            TempData["ErrorMessage"] = "The requested NGO record was not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new NgoFormViewModel
        {
            Id = ngo.Id,
            Name = ngo.Name,
            RegistrationNumber = ngo.RegistrationNumber,
            Description = ngo.Description,
            Address = ngo.Address,
            Email = ngo.Email,
            Phone = ngo.Phone,
            WebsiteUrl = ngo.WebsiteUrl,
            IsActive = ngo.IsActive,
            ExistingLogoPath = ngo.LogoPath
        };

        return View("~/Views/Admin/Ngos/Edit.cshtml", model);
    }

    // =========================================================================
    // POST: /Admin/Ngos/Edit/{id}
    // =========================================================================
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NgoFormViewModel model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid request: ID mismatch.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Ngos/Edit.cshtml", model);

        var ngo = await _context.NGOs.FindAsync(id);
        if (ngo == null)
        {
            TempData["ErrorMessage"] = "The requested NGO record was not found.";
            return RedirectToAction(nameof(Index));
        }

        ngo.Name = model.Name;
        ngo.RegistrationNumber = model.RegistrationNumber;
        ngo.Description = model.Description;
        ngo.Address = model.Address;
        ngo.Email = model.Email;
        ngo.Phone = model.Phone;
        ngo.WebsiteUrl = model.WebsiteUrl;
        ngo.IsActive = model.IsActive;

        // Handle logo upload — preserve existing if no new file uploaded
        if (model.LogoFile != null && model.LogoFile.Length > 0)
        {
            var (path, error) = await _imageUploadService.SaveImageAsync(model.LogoFile, "ngos");
            if (error != null)
            {
                ModelState.AddModelError("LogoFile", error);
                return View("~/Views/Admin/Ngos/Edit.cshtml", model);
            }

            // Delete old logo if it existed
            if (!string.IsNullOrEmpty(ngo.LogoPath))
                _imageUploadService.DeleteImage(ngo.LogoPath);

            ngo.LogoPath = path;
        }
        // If no new file and ExistingLogoPath is explicitly cleared, keep whatever is in the entity

        await _context.SaveChangesAsync();

        _logger.LogInformation("NGO '{Name}' (ID: {Id}) updated by {User}", ngo.Name, ngo.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"NGO \"{ngo.Name}\" has been updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Ngos/Delete/{id}
    // =========================================================================
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ngo = await _context.NGOs
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (ngo == null)
        {
            TempData["ErrorMessage"] = "The requested NGO record was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Ngos/Delete.cshtml", ngo);
    }

    // =========================================================================
    // POST: /Admin/Ngos/Delete/{id}
    // =========================================================================
    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ngo = await _context.NGOs.FindAsync(id);
        if (ngo == null)
        {
            TempData["ErrorMessage"] = "The requested NGO record was not found.";
            return RedirectToAction(nameof(Index));
        }

        // Check for related causes (Restrict delete configured in DbContext)
        var hasCauses = await _context.Causes.AnyAsync(c => c.NgoId == ngo.Id);
        if (hasCauses)
        {
            TempData["ErrorMessage"] = $"Cannot delete NGO \"{ngo.Name}\" because it has associated causes. Remove or reassign the causes first.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        // Delete logo file if it exists
        if (!string.IsNullOrEmpty(ngo.LogoPath))
            _imageUploadService.DeleteImage(ngo.LogoPath);

        _context.NGOs.Remove(ngo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("NGO '{Name}' (ID: {Id}) deleted by {User}", ngo.Name, ngo.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"NGO \"{ngo.Name}\" has been deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
