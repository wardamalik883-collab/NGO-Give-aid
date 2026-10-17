using System.ComponentModel.DataAnnotations;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// View model for inviting friends and community members.
/// Implements Task 15 & Task 18 requirements.
/// </summary>
public class InvitationFormViewModel
{
    [Required(ErrorMessage = "Recipient email address is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(100, ErrorMessage = "Email address cannot exceed 100 characters.")]
    [Display(Name = "Recipient Email Address")]
    public string RecipientEmail { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Personal message cannot exceed 500 characters.")]
    [Display(Name = "Personal Message (Optional)")]
    public string? Message { get; set; }
}
