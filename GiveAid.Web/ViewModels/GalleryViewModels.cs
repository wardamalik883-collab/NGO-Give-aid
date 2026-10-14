using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using GiveAid.Web.Models;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for uploading and editing gallery images in the admin portal.
/// Supports optional linkage to an accredited welfare programme.
/// </summary>
public class GalleryImageFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Image title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    [Display(Name = "Image Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Caption cannot exceed 250 characters.")]
    [Display(Name = "Caption & Narrative")]
    public string? Caption { get; set; }

    [Required(ErrorMessage = "Accessibility alternative text (Alt Text) is required.")]
    [StringLength(150, ErrorMessage = "Alt text cannot exceed 150 characters.")]
    [Display(Name = "Alternative Text (Accessibility / WCAG AA)")]
    public string AltText { get; set; } = string.Empty;

    [Display(Name = "Associated Programme (Optional)")]
    public int? ProgrammeId { get; set; }

    [Display(Name = "Photo File")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImagePath { get; set; }

    public List<SelectListItem> AvailableProgrammes { get; set; } = new();
}

/// <summary>
/// ViewModel for the public photo gallery grid with programme filter and lightbox previews.
/// </summary>
public class GalleryIndexViewModel
{
    public List<GalleryImage> Images { get; set; } = new();
    public List<Programme> Programmes { get; set; } = new();
    public int? SelectedProgrammeId { get; set; }
}
