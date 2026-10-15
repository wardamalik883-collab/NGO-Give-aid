using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using GiveAid.Web.Models;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for the public donation form.
/// Applies strict server-side validation rules in accordance with project requirements.
/// Real credit card numbers or credentials are NEVER collected or stored.
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
/// ViewModel representing the simulated demo payment step.
/// Deterministic outcomes allow clear testing of success and failure without real financial risk.
/// </summary>
public class SimulatedPaymentViewModel
{
    [Required]
    public int CauseId { get; set; }

    public string CauseTitle { get; set; } = string.Empty;
    public string NgoName { get; set; } = string.Empty;

    [Required]
    [Range(1, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(100)]
    public string DonorName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string DonorEmail { get; set; } = string.Empty;

    public string? DonorPhone { get; set; }

    /// <summary>
    /// Explicit deterministic test outcome: "Success" or "Failure".
    /// </summary>
    [Required]
    public string SimulatedOutcome { get; set; } = "Success";

    /// <summary>
    /// Optional failure simulation reason or test notes.
    /// </summary>
    public string? SimulatedNotes { get; set; }

    /// <summary>
    /// Unique token to prevent duplicate form submission.
    /// </summary>
    public string IdempotencyToken { get; set; } = Guid.NewGuid().ToString("N");
}

/// <summary>
/// ViewModel for displaying donation receipt / confirmation.
/// </summary>
public class DonationReceiptViewModel
{
    public int DonationId { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DonationDate { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public bool IsSimulated { get; set; } = true;
    public int CauseId { get; set; }
    public string CauseTitle { get; set; } = string.Empty;
    public string NgoName { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public string DonorEmail { get; set; } = string.Empty;
    public string? DonorPhone { get; set; }
    public string? PaymentNotes { get; set; }
    public bool IsAdminView { get; set; } = false;
    public bool IsSuccessful => string.Equals(PaymentStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                             || string.Equals(PaymentStatus, "Successful", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// ViewModel for authenticated user donation history page.
/// </summary>
public class UserDonationHistoryViewModel
{
    public List<Donation> Donations { get; set; } = new();
    public decimal TotalContributed { get; set; }
    public int TotalDonationsCount { get; set; }
    public int SuccessfulDonationsCount { get; set; }
    public int UniqueCausesSupportedCount { get; set; }
}

/// <summary>
/// ViewModel for administrator donation management list.
/// </summary>
public class AdminDonationListViewModel
{
    public List<Donation> Donations { get; set; } = new();
    public string? StatusFilter { get; set; }
    public int? CauseFilter { get; set; }
    public string? SearchQuery { get; set; }
    public decimal TotalAmountRaised { get; set; }
    public int TotalCount { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public List<SelectListItem> CausesList { get; set; } = new();
}

/// <summary>
/// Preserved for backward compatibility from Task 09 review state.
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
