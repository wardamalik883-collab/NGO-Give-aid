using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public controller for exploring community photo gallery.
/// Implements FR-042: Responsive Photo Gallery with Programme Filtering.
/// Implements FR-044: Modal Lightbox Image Inspection with accessibility captions.
/// </summary>
public class GalleryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GalleryController> _logger;

    public GalleryController(ApplicationDbContext context, ILogger<GalleryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Gallery or /Gallery/Index?programmeId={id}
    // Displays photo gallery with optional programme filter.
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Index(int? programmeId)
    {
        // Fetch all programmes that have images, or all programmes to display filter buttons
        var programmes = await _context.Programmes
            .OrderBy(p => p.Title)
            .AsNoTracking()
            .ToListAsync();

        var query = _context.GalleryImages
            .Include(g => g.Programme)
            .AsNoTracking()
            .AsQueryable();

        if (programmeId.HasValue && programmeId.Value > 0)
        {
            query = query.Where(g => g.ProgrammeId == programmeId.Value);
        }

        var images = await query
            .OrderBy(g => g.Id)
            .ToListAsync();

        var viewModel = new GalleryIndexViewModel
        {
            Images = images,
            Programmes = programmes,
            SelectedProgrammeId = programmeId
        };

        return View(viewModel);
    }
}
