using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Admin-only controller for managing welfare programmes (CRUD).
/// Implements FR-045: Programme Lifecycle Management.
/// Implements FR-047: Banner Upload with strict file validation.
/// Protected by [Authorize(Roles = "Admin")].
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Programmes")]
public class AdminProgrammesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ImageUploadService _imageUploadService;
    private readonly ILogger<AdminProgrammesController> _logger;

    public AdminProgrammesController(
        ApplicationDbContext context,
        ImageUploadService imageUploadService,
        ILogger<AdminProgrammesController> logger)
    {
        _context = context;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Admin/Programmes
    // =========================================================================
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var programmes = await _context.Programmes
            .Include(p => p.GalleryImages)
            .OrderByDescending(p => p.EventDate)
            .AsNoTracking()
            .ToListAsync();

        return View("~/Views/Admin/Programmes/Index.cshtml", programmes);
    }

    // =========================================================================
    // GET: /Admin/Programmes/Details/{id}
    // =========================================================================
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var programme = await _context.Programmes
            .Include(p => p.GalleryImages)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (programme == null)
        {
            TempData["ErrorMessage"] = "The requested programme record was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Programmes/Details.cshtml", programme);
    }

    // =========================================================================
    // GET: /Admin/Programmes/Create
    // =========================================================================
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Programmes/Create.cshtml", new ProgrammeFormViewModel());
    }

    // =========================================================================
    // POST: /Admin/Programmes/Create
    // =========================================================================
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProgrammeFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Programmes/Create.cshtml", model);

        var programme = new Programme
        {
            Title = model.Title,
            Description = model.Description,
            EventDate = model.StartDate,
            EndDate = model.EndDate,
            Location = model.Location,
            CoordinatorName = model.CoordinatorName,
            CoordinatorContact = model.CoordinatorContact,
            Status = model.Status,
            CreatedAt = DateTime.UtcNow
        };

        // Handle banner image upload
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var (path, error) = await _imageUploadService.SaveImageAsync(model.ImageFile, "programmes", 5 * 1024 * 1024);
            if (error != null)
            {
                ModelState.AddModelError("ImageFile", error);
                return View("~/Views/Admin/Programmes/Create.cshtml", model);
            }
            programme.ImagePath = path;
        }

        _context.Programmes.Add(programme);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Programme '{Title}' (ID: {Id}) created by {User}", programme.Title, programme.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Programme \"{programme.Title}\" has been created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Programmes/Edit/{id}
    // =========================================================================
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var programme = await _context.Programmes.FindAsync(id);
        if (programme == null)
        {
            TempData["ErrorMessage"] = "The requested programme record was not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ProgrammeFormViewModel
        {
            Id = programme.Id,
            Title = programme.Title,
            Description = programme.Description,
            StartDate = programme.StartDate,
            EndDate = programme.EndDate,
            Location = programme.Location,
            CoordinatorName = programme.CoordinatorName,
            CoordinatorContact = programme.CoordinatorContact,
            Status = programme.Status,
            ExistingImagePath = programme.ImagePath
        };

        return View("~/Views/Admin/Programmes/Edit.cshtml", model);
    }

    // =========================================================================
    // POST: /Admin/Programmes/Edit/{id}
    // =========================================================================
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProgrammeFormViewModel model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid request: ID mismatch.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
            return View("~/Views/Admin/Programmes/Edit.cshtml", model);

        var programme = await _context.Programmes.FindAsync(id);
        if (programme == null)
        {
            TempData["ErrorMessage"] = "The requested programme record was not found.";
            return RedirectToAction(nameof(Index));
        }

        programme.Title = model.Title;
        programme.Description = model.Description;
        programme.EventDate = model.StartDate;
        programme.EndDate = model.EndDate;
        programme.Location = model.Location;
        programme.CoordinatorName = model.CoordinatorName;
        programme.CoordinatorContact = model.CoordinatorContact;
        programme.Status = model.Status;
        programme.UpdatedAt = DateTime.UtcNow;

        // Handle banner image upload — preserve existing if no new file uploaded
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var (path, error) = await _imageUploadService.SaveImageAsync(model.ImageFile, "programmes", 5 * 1024 * 1024);
            if (error != null)
            {
                ModelState.AddModelError("ImageFile", error);
                return View("~/Views/Admin/Programmes/Edit.cshtml", model);
            }

            // Delete old banner if it existed
            if (!string.IsNullOrEmpty(programme.ImagePath))
                _imageUploadService.DeleteImage(programme.ImagePath);

            programme.ImagePath = path;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Programme '{Title}' (ID: {Id}) updated by {User}", programme.Title, programme.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Programme \"{programme.Title}\" has been updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Programmes/Delete/{id}
    // =========================================================================
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var programme = await _context.Programmes
            .Include(p => p.GalleryImages)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (programme == null)
        {
            TempData["ErrorMessage"] = "The requested programme record was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Programmes/Delete.cshtml", programme);
    }

    // =========================================================================
    // POST: /Admin/Programmes/Delete/{id}
    // =========================================================================
    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var programme = await _context.Programmes
            .Include(p => p.GalleryImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (programme == null)
        {
            TempData["ErrorMessage"] = "The requested programme record was not found.";
            return RedirectToAction(nameof(Index));
        }

        // Delete banner image file if exists
        if (!string.IsNullOrEmpty(programme.ImagePath))
        {
            _imageUploadService.DeleteImage(programme.ImagePath);
        }

        // Note: Related GalleryImages have OnDelete(DeleteBehavior.SetNull)
        // Their ProgrammeId is set to NULL, preserving the photos and their physical files safely.
        _context.Programmes.Remove(programme);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Programme '{Title}' (ID: {Id}) deleted by {User}", programme.Title, id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Programme \"{programme.Title}\" has been deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
