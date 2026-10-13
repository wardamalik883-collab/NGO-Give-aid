using System.ComponentModel.DataAnnotations;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for creating and editing NGO records in the admin panel.
/// Includes an IFormFile property for logo upload and validation attributes.
/// </summary>
public class NgoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "NGO Name is required.")]
    [StringLength(150, ErrorMessage = "NGO Name cannot exceed 150 characters.")]
    [Display(Name = "NGO Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Registration Number")]
    public string? RegistrationNumber { get; set; }

    [StringLength(1000)]
    [Display(Name = "Mission & Description")]
    public string? Description { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [StringLength(250)]
    [Display(Name = "Website URL")]
    public string? WebsiteUrl { get; set; }

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
