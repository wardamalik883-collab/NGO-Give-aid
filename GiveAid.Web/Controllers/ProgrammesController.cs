using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public controller for browsing welfare programmes and community events.
/// Implements FR-038: Public Welfare Programme Directory.
/// Implements FR-039: Programme Detail View with related gallery imagery.
/// Separates programmes into Upcoming and Past events using event dates.
/// </summary>
public class ProgrammesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProgrammesController> _logger;

    public ProgrammesController(ApplicationDbContext context, ILogger<ProgrammesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Programmes or /Programmes/Index
    // Displays upcoming and past welfare programmes in organized sections.
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;

        // Fetch all programmes
        var allProgrammes = await _context.Programmes
            .AsNoTracking()
            .ToListAsync();

        // Upcoming programmes: StartDate/EventDate or EndDate is today or in the future
        // Sorted by start date ascending (nearest first)
        var upcoming = allProgrammes
            .Where(p => (p.EndDate ?? p.EventDate).Date >= today)
            .OrderBy(p => p.EventDate)
            .ToList();

        // Past programmes: EndDate (or StartDate if no EndDate) occurred before today
        // Sorted by start date descending (most recent first)
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
    // Displays full details for a specific programme, including coordinator details
    // and related gallery imagery.
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
}
