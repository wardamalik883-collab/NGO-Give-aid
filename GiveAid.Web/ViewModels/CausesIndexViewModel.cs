namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel representing the public Causes catalog listing page.
/// Supports search filtering, progress tracking, and empty-state messaging.
/// </summary>
public class CausesIndexViewModel
{
    public List<CauseCardItemViewModel> Causes { get; set; } = new();
    public string? SearchQuery { get; set; }
    public int TotalActiveCauses => Causes.Count;
}

public class CauseCardItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public int ProgressPercentage => TargetAmount > 0 ? (int)Math.Min(100, Math.Round((CurrentAmount / TargetAmount) * 100)) : 0;
    public string? ImagePath { get; set; }
    public string NgoName { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}
