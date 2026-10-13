namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel representing a specific Cause details page.
/// Encapsulates full narrative description, fundraising targets, and organizing NGO profile.
/// </summary>
public class CauseDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string FullDescription { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public int ProgressPercentage => TargetAmount > 0 ? (int)Math.Min(100, Math.Round((CurrentAmount / TargetAmount) * 100)) : 0;
    public decimal RemainingAmount => Math.Max(0, TargetAmount - CurrentAmount);
    public string? ImagePath { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Associated NGO Profile
    public int NgoId { get; set; }
    public string NgoName { get; set; } = string.Empty;
    public string? NgoRegistrationNumber { get; set; }
    public string? NgoDescription { get; set; }
    public string? NgoAddress { get; set; }
    public string? NgoEmail { get; set; }
    public string? NgoPhone { get; set; }
    public string? NgoWebsiteUrl { get; set; }
}
