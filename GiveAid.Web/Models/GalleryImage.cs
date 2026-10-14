using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAid.Web.Models;

/// <summary>
/// Represents photographic evidence and impact documentation for community activities.
/// Can be associated with a specific Programme or stand independently in the public gallery.
/// </summary>
public class GalleryImage
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Image title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    [Display(Name = "Image Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Caption & Description")]
    public string? Caption { get; set; }

    [Required(ErrorMessage = "Alt text is required for accessibility (WCAG AA compliance).")]
    [StringLength(150, ErrorMessage = "Alt text cannot exceed 150 characters.")]
    [Display(Name = "Alternative Text (Accessibility)")]
    public string AltText { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    [Display(Name = "Server Image Storage Path")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Alias property for ImagePath to satisfy Task 11 requirements.
    /// </summary>
    [NotMapped]
    [Display(Name = "Image Path")]
    public string ImagePath
    {
        get => FilePath;
        set => FilePath = value;
    }

    [Display(Name = "Upload Timestamp (UTC)")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Optional foreign key to associated programme
    [Display(Name = "Associated Programme")]
    public int? ProgrammeId { get; set; }

    [ForeignKey("ProgrammeId")]
    public Programme? Programme { get; set; }
}
