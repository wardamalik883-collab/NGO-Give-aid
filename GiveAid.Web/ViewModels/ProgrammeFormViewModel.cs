using System.ComponentModel.DataAnnotations;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for creating and editing welfare programmes in the admin panel.
/// Implements custom date validation so EndDate cannot precede StartDate.
/// Supports banner image upload with server-side validation.
/// </summary>
public class ProgrammeFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Programme title is required.")]
    [StringLength(150, ErrorMessage = "Programme title cannot exceed 150 characters.")]
    [Display(Name = "Programme Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Programme description is required.")]
    [Display(Name = "Objectives & Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date & time is required.")]
    [Display(Name = "Start Date & Time")]
    [DataType(DataType.DateTime)]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date.AddDays(7).AddHours(10); // default to 10:00 AM in 7 days

    [Display(Name = "End Date & Time (Optional)")]
    [DataType(DataType.DateTime)]
    public DateTime? EndDate { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
    [Display(Name = "Venue / Location")]
    public string Location { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Coordinator name cannot exceed 100 characters.")]
    [Display(Name = "Lead Coordinator Name")]
    public string? CoordinatorName { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(50, ErrorMessage = "Coordinator contact cannot exceed 50 characters.")]
    [Display(Name = "Coordinator Contact")]
    public string? CoordinatorContact { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(25)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

    [Display(Name = "Banner Image")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImagePath { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate.HasValue && EndDate.Value < StartDate)
        {
            yield return new ValidationResult(
                "End date cannot be earlier than start date.",
                new[] { nameof(EndDate) });
        }
    }
}
