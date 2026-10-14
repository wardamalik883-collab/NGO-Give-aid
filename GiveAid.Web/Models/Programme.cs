using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAid.Web.Models;

/// <summary>
/// Represents a community welfare programme, relief camp, or humanitarian event.
/// Supports both upcoming and past event filtering, optional end dates, coordinators, and gallery links.
/// </summary>
public class Programme
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Programme title is required.")]
    [StringLength(150, ErrorMessage = "Programme title cannot exceed 150 characters.")]
    [Display(Name = "Programme Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Programme description is required.")]
    [Display(Name = "Event Objectives & Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event start date is required.")]
    [Display(Name = "Event / Start Date & Time (UTC)")]
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Alias property for StartDate to satisfy Task 11 requirements.
    /// </summary>
    [NotMapped]
    [Display(Name = "Start Date & Time (UTC)")]
    public DateTime StartDate
    {
        get => EventDate;
        set => EventDate = value;
    }

    [Display(Name = "End Date & Time (UTC, Optional)")]
    public DateTime? EndDate { get; set; }

    [Required(ErrorMessage = "Venue location is required.")]
    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    [Display(Name = "Venue / City Location")]
    public string Location { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Lead Coordinator Name")]
    public string? CoordinatorName { get; set; }

    [StringLength(50)]
    [Display(Name = "Coordinator Contact")]
    public string? CoordinatorContact { get; set; }

    [StringLength(250)]
    [Display(Name = "Event Banner Image Path")]
    public string? BannerImagePath { get; set; }

    /// <summary>
    /// Alias property for ImagePath to satisfy Task 11 requirements.
    /// </summary>
    [NotMapped]
    [Display(Name = "Image Path")]
    public string? ImagePath
    {
        get => BannerImagePath;
        set => BannerImagePath = value;
    }

    [Required]
    [StringLength(25)]
    [Display(Name = "Programme Status")]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

    [Display(Name = "Created Date (UTC)")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated (UTC)")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ProgrammeInterest> Interests { get; set; } = new List<ProgrammeInterest>();
    public ICollection<GalleryImage> GalleryImages { get; set; } = new List<GalleryImage>();
}
