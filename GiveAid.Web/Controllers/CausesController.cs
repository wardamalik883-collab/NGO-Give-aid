using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;
using GiveAid.Web.ViewModels;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public controller for browsing charitable causes and campaign details.
/// Supports search filtering, progress tracking, and navigation into the donation form.
/// </summary>
public class CausesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CausesController> _logger;

    public CausesController(ApplicationDbContext context, ILogger<CausesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Causes or /Causes/Index
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Index(string? q = null)
    {
        var query = _context.Causes
            .Include(c => c.Ngo)
            .Where(c => c.IsActive)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var search = q.Trim();
            query = query.Where(c => c.Title.Contains(search) || c.ShortDescription.Contains(search));
        }

        var causes = await query
            .OrderByDescending(c => c.IsFeatured)
            .ThenByDescending(c => c.CreatedAt)
            .Select(c => new CauseCardItemViewModel
            {
                Id = c.Id,
                Title = c.Title,
                ShortDescription = c.ShortDescription,
                TargetAmount = c.TargetAmount,
                CurrentAmount = c.CurrentAmount,
                ImagePath = c.ImagePath,
                NgoName = c.Ngo != null ? c.Ngo.Name : "Give-AID Partner",
                IsFeatured = c.IsFeatured,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        var model = new CausesIndexViewModel
        {
            Causes = causes,
            SearchQuery = q
        };

        return View(model);
    }

    // =========================================================================
    // GET: /Causes/Details/{id}
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cause = await _context.Causes
            .Include(c => c.Ngo)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id.Value && c.IsActive);

        if (cause == null)
        {
            return NotFound();
        }

        var model = new CauseDetailsViewModel
        {
            Id = cause.Id,
            Title = cause.Title,
            ShortDescription = cause.ShortDescription,
            FullDescription = cause.FullDescription,
            TargetAmount = cause.TargetAmount,
            CurrentAmount = cause.CurrentAmount,
            ImagePath = cause.ImagePath,
            IsFeatured = cause.IsFeatured,
            StartDate = cause.StartDate,
            EndDate = cause.EndDate,
            CreatedAt = cause.CreatedAt,
            NgoId = cause.NgoId,
            NgoName = cause.Ngo?.Name ?? "Give-AID Partner Organization",
            NgoRegistrationNumber = cause.Ngo?.RegistrationNumber,
            NgoDescription = cause.Ngo?.Description,
            NgoAddress = cause.Ngo?.Address,
            NgoEmail = cause.Ngo?.Email,
            NgoPhone = cause.Ngo?.Phone,
            NgoWebsiteUrl = cause.Ngo?.WebsiteUrl
        };

        return View(model);
    }
}
