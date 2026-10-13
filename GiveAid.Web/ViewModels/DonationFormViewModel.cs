using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for the public donation form.
/// Applies strict server-side validation rules in accordance with project requirements.
/// Task 09 scope: UI and form validation only. Payment processing handled in Task 13.
/// </summary>
public class DonationFormViewModel
{
    [Required(ErrorMessage = "Please select a cause to support.")]
    [Display(Name = "Target Cause")]
    public int? CauseId { get; set; }

    // Preselected Cause contextual info
    public string? SelectedCauseTitle { get; set; }
    public string? SelectedCauseNgoName { get; set; }
    public decimal? SelectedCauseTargetAmount { get; set; }
    public decimal? SelectedCauseCurrentAmount { get; set; }

    [Required(ErrorMessage = "Please enter a donation amount.")]
    [Range(1, 10000000, ErrorMessage = "Donation amount must be between PKR 1 and PKR 10,000,000.")]
    [Display(Name = "Donation Amount (PKR)")]
    public decimal? Amount { get; set; }

    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, ErrorMessage = "Donor name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string DonorName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [Display(Name = "Email Address")]
    public string DonorEmail { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid telephone number.")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    [Display(Name = "Phone Number (Optional)")]
    public string? DonorPhone { get; set; }

    // Available causes dropdown options
    public List<SelectListItem> AvailableCauses { get; set; } = new();
}

/// <summary>
/// ViewModel presenting the validated donation review/confirmation state.
/// Clarifies that actual payment processing will occur in Task 13.
/// </summary>
public class DonationConfirmationViewModel
{
    public int CauseId { get; set; }
    public string CauseTitle { get; set; } = string.Empty;
    public string NgoName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string DonorEmail { get; set; } = string.Empty;
    public string? DonorPhone { get; set; }
    public DateTime ValidatedAtUtc { get; set; } = DateTime.UtcNow;
}
