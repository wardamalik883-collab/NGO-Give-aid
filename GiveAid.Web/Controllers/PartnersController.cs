using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiveAid.Web.Data;

namespace GiveAid.Web.Controllers;

/// <summary>
/// Public controller for browsing affiliated NGOs and corporate partners.
/// Implements FR-033: Public NGO & Partner Showcase.
/// Implements FR-034: Partner/NGO Detail Profile.
/// No authentication required — public visitors can browse freely.
/// </summary>
public class PartnersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PartnersController> _logger;

    public PartnersController(ApplicationDbContext context, ILogger<PartnersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // =========================================================================
    // GET: /Partners or /Partners/Index
    // Public listing of all active NGOs and Partners
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var ngos = await _context.NGOs
            .Where(n => n.IsActive)
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        var partners = await _context.Partners
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        ViewBag.NGOs = ngos;
        ViewBag.Partners = partners;

        return View();
    }

    // =========================================================================
    // GET: /Partners/NgoDetails/{id}
    // Public detail view for a specific NGO
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> NgoDetails(int? id)
    {
        if (id == null)
            return NotFound();

        var ngo = await _context.NGOs
            .Include(n => n.Causes.Where(c => c.IsActive))
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id);

        if (ngo == null)
            return NotFound();

        return View(ngo);
    }

    // =========================================================================
    // GET: /Partners/PartnerDetails/{id}
    // Public detail view for a specific Partner
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> PartnerDetails(int? id)
    {
        if (id == null)
            return NotFound();

        var partner = await _context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (partner == null)
            return NotFound();

        return View(partner);
    }
}
