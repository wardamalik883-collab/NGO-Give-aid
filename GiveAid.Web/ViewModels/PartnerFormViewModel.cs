using System.ComponentModel.DataAnnotations;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for creating and editing Partner records in the admin panel.
/// Includes an IFormFile property for logo upload and validation attributes.
/// </summary>
public class PartnerFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Partner Name is required.")]
    [StringLength(150, ErrorMessage = "Partner Name cannot exceed 150 characters.")]
    [Display(Name = "Partner Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Partnership Type")]
    public string PartnerType { get; set; } = "Corporate Sponsor";

    [StringLength(500)]
    [Display(Name = "Partner Synopsis")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [StringLength(250)]
    [Display(Name = "Website URL")]
    public string? WebsiteUrl { get; set; }

    [Display(Name = "Display Order")]
    [Range(0, 1000, ErrorMessage = "Display order must be between 0 and 1000.")]
    public int DisplayOrder { get; set; } = 0;

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional logo image upload. Validated server-side for extension and size.
    /// </summary>
    [Display(Name = "Logo Image")]
    public IFormFile? LogoFile { get; set; }

    /// <summary>
    /// Stores the current logo path when editing, so it can be preserved if no new file is uploaded.
    /// </summary>
    public string? ExistingLogoPath { get; set; }
}
