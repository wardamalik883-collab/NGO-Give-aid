using GiveAid.Web.Models;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for public welfare programmes catalog page.
/// Segregates programmes into Upcoming and Past sections based on event dates.
/// </summary>
public class ProgrammesIndexViewModel
{
    public List<Programme> UpcomingProgrammes { get; set; } = new();
    public List<Programme> PastProgrammes { get; set; } = new();

    public bool HasAnyProgrammes => UpcomingProgrammes.Any() || PastProgrammes.Any();
}

/// <summary>
/// ViewModel for public programme detail page.
/// Includes agenda, venue, coordinator contact, and related gallery photos.
/// </summary>
public class ProgrammeDetailsViewModel
{
    public Programme Programme { get; set; } = null!;
    public List<GalleryImage> RelatedGalleryImages { get; set; } = new();
    public bool IsUpcoming { get; set; }
}
