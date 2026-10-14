using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.Models;
using GiveAid.Web.Services;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Admin-only controller for managing community photo gallery records and uploads.
/// Implements FR-046: Gallery Image Asset Administration.
/// Implements FR-048: Physical File Cleanup on image deletion.
/// Protected by [Authorize(Roles = "Admin")].
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Gallery")]
public class AdminGalleryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ImageUploadService _imageUploadService;
    private readonly ILogger<AdminGalleryController> _logger;

    public AdminGalleryController(
        ApplicationDbContext context,
        ImageUploadService imageUploadService,
        ILogger<AdminGalleryController> logger)
    {
        _context = context;
        _imageUploadService = imageUploadService;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Admin/Gallery
    // =========================================================================
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var images = await _context.GalleryImages
            .Include(g => g.Programme)
            .OrderByDescending(g => g.UploadedAt)
            .AsNoTracking()
            .ToListAsync();

        return View("~/Views/Admin/Gallery/Index.cshtml", images);
    }

    // =========================================================================
    // GET: /Admin/Gallery/Details/{id}
    // =========================================================================
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var image = await _context.GalleryImages
            .Include(g => g.Programme)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (image == null)
        {
            TempData["ErrorMessage"] = "The requested gallery image was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Gallery/Details.cshtml", image);
    }

    // =========================================================================
    // GET: /Admin/Gallery/Upload
    // =========================================================================
    [HttpGet("Upload")]
    public async Task<IActionResult> Upload()
    {
        var model = new GalleryImageFormViewModel
        {
            AvailableProgrammes = await GetAvailableProgrammesAsync()
        };

        return View("~/Views/Admin/Gallery/Upload.cshtml", model);
    }

    // =========================================================================
    // POST: /Admin/Gallery/Upload
    // =========================================================================
    [HttpPost("Upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(GalleryImageFormViewModel model)
    {
        if (model.ImageFile == null || model.ImageFile.Length == 0)
        {
            ModelState.AddModelError("ImageFile", "Please select a photo file to upload.");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableProgrammes = await GetAvailableProgrammesAsync();
            return View("~/Views/Admin/Gallery/Upload.cshtml", model);
        }

        // Validate programme if provided
        if (model.ProgrammeId.HasValue && model.ProgrammeId.Value > 0)
        {
            var programmeExists = await _context.Programmes.AnyAsync(p => p.Id == model.ProgrammeId.Value);
            if (!programmeExists)
            {
                ModelState.AddModelError("ProgrammeId", "The selected programme does not exist.");
                model.AvailableProgrammes = await GetAvailableProgrammesAsync();
                return View("~/Views/Admin/Gallery/Upload.cshtml", model);
            }
        }
        else
        {
            model.ProgrammeId = null;
        }

        // Save image file
        var (path, error) = await _imageUploadService.SaveImageAsync(model.ImageFile!, "gallery", 5 * 1024 * 1024);
        if (error != null)
        {
            ModelState.AddModelError("ImageFile", error);
            model.AvailableProgrammes = await GetAvailableProgrammesAsync();
            return View("~/Views/Admin/Gallery/Upload.cshtml", model);
        }

        var galleryImage = new GalleryImage
        {
            Title = model.Title,
            Caption = model.Caption,
            AltText = model.AltText,
            ProgrammeId = model.ProgrammeId,
            FilePath = path!,
            UploadedAt = DateTime.UtcNow
        };

        _context.GalleryImages.Add(galleryImage);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Gallery image '{Title}' (ID: {Id}) uploaded by {User}", galleryImage.Title, galleryImage.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Photo \"{galleryImage.Title}\" was uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Gallery/Edit/{id}
    // =========================================================================
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var image = await _context.GalleryImages.FindAsync(id);
        if (image == null)
        {
            TempData["ErrorMessage"] = "The requested gallery image was not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = new GalleryImageFormViewModel
        {
            Id = image.Id,
            Title = image.Title,
            Caption = image.Caption,
            AltText = image.AltText,
            ProgrammeId = image.ProgrammeId,
            ExistingImagePath = image.ImagePath,
            AvailableProgrammes = await GetAvailableProgrammesAsync()
        };

        return View("~/Views/Admin/Gallery/Edit.cshtml", model);
    }

    // =========================================================================
    // POST: /Admin/Gallery/Edit/{id}
    // =========================================================================
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GalleryImageFormViewModel model)
    {
        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid request: ID mismatch.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            model.AvailableProgrammes = await GetAvailableProgrammesAsync();
            return View("~/Views/Admin/Gallery/Edit.cshtml", model);
        }

        var image = await _context.GalleryImages.FindAsync(id);
        if (image == null)
        {
            TempData["ErrorMessage"] = "The requested gallery image was not found.";
            return RedirectToAction(nameof(Index));
        }

        // Validate programme if provided
        if (model.ProgrammeId.HasValue && model.ProgrammeId.Value > 0)
        {
            var programmeExists = await _context.Programmes.AnyAsync(p => p.Id == model.ProgrammeId.Value);
            if (!programmeExists)
            {
                ModelState.AddModelError("ProgrammeId", "The selected programme does not exist.");
                model.AvailableProgrammes = await GetAvailableProgrammesAsync();
                return View("~/Views/Admin/Gallery/Edit.cshtml", model);
            }
            image.ProgrammeId = model.ProgrammeId.Value;
        }
        else
        {
            image.ProgrammeId = null;
        }

        image.Title = model.Title;
        image.Caption = model.Caption;
        image.AltText = model.AltText;

        // If replacement image provided
        if (model.ImageFile != null && model.ImageFile.Length > 0)
        {
            var (path, error) = await _imageUploadService.SaveImageAsync(model.ImageFile, "gallery", 5 * 1024 * 1024);
            if (error != null)
            {
                ModelState.AddModelError("ImageFile", error);
                model.AvailableProgrammes = await GetAvailableProgrammesAsync();
                return View("~/Views/Admin/Gallery/Edit.cshtml", model);
            }

            // Remove old file
            if (!string.IsNullOrEmpty(image.FilePath))
                _imageUploadService.DeleteImage(image.FilePath);

            image.FilePath = path!;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Gallery image '{Title}' (ID: {Id}) updated by {User}", image.Title, image.Id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Photo \"{image.Title}\" was updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // GET: /Admin/Gallery/Delete/{id}
    // =========================================================================
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var image = await _context.GalleryImages
            .Include(g => g.Programme)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (image == null)
        {
            TempData["ErrorMessage"] = "The requested gallery image was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Gallery/Delete.cshtml", image);
    }

    // =========================================================================
    // POST: /Admin/Gallery/Delete/{id}
    // =========================================================================
    [HttpPost("Delete/{id:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var image = await _context.GalleryImages.FindAsync(id);
        if (image == null)
        {
            TempData["ErrorMessage"] = "The requested gallery image was not found.";
            return RedirectToAction(nameof(Index));
        }

        // Delete physical file from disk
        if (!string.IsNullOrEmpty(image.FilePath))
        {
            _imageUploadService.DeleteImage(image.FilePath);
        }

        _context.GalleryImages.Remove(image);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Gallery image '{Title}' (ID: {Id}) deleted by {User}", image.Title, id, User.Identity?.Name);
        TempData["SuccessMessage"] = $"Photo \"{image.Title}\" was permanently deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetAvailableProgrammesAsync()
    {
        var programmes = await _context.Programmes
            .OrderBy(p => p.Title)
            .AsNoTracking()
            .ToListAsync();

        var list = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "— General Community (No Programme) —" }
        };

        list.AddRange(programmes.Select(p => new SelectListItem
        {
            Value = p.Id.ToString(),
            Text = $"{p.Title} ({p.EventDate:dd MMM yyyy})"
        }));

        return list;
    }
}
