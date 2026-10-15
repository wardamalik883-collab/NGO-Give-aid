using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAid.Web.Models;

/// <summary>
/// Represents a simulated financial contribution.
/// STRICT RULE: Real credit/debit card numbers, CVVs, or card credentials are NEVER stored.
/// </summary>
public class Donation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(60)]
    [Display(Name = "Transaction Reference")]
    public string TransactionReference { get; set; } = string.Empty;

    [Required]
    [Range(1, 10000000, ErrorMessage = "Donation amount must be between 1 and 10,000,000.")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Donated Amount")]
    public decimal Amount { get; set; }

    [Display(Name = "Donation Date (UTC)")]
    public DateTime DonationDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100, ErrorMessage = "Donor name cannot exceed 100 characters.")]
    [Display(Name = "Donor Name")]
    public string DonorName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    [Display(Name = "Donor Email Address")]
    public string DonorEmail { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    [Display(Name = "Donor Phone Number")]
    public string? DonorPhone { get; set; }

    [Required]
    [StringLength(25)]
    [Display(Name = "Payment Status")]
    public string PaymentStatus { get; set; } = "Completed"; // Completed, Failed, Pending

    [Display(Name = "Simulated Demo Transaction")]
    public bool IsSimulated { get; set; } = true;

    // Associated charitable cause
    [Required]
    [Display(Name = "Target Cause")]
    public int CauseId { get; set; }

    [ForeignKey("CauseId")]
    public Cause? Cause { get; set; }

    // Optional link to registered member (ASP.NET Core Identity string User ID)
    [StringLength(450)]
    [Display(Name = "Registered User ID")]
    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    [Display(Name = "Updated Date (UTC)")]
    public DateTime? UpdatedAt { get; set; }

    [StringLength(500)]
    [Display(Name = "Simulated Payment Notes")]
    public string? PaymentNotes { get; set; }

    // Property aliases for roadmap and conventions
    [NotMapped]
    public string Reference
    {
        get => TransactionReference;
        set => TransactionReference = value;
    }

    [NotMapped]
    public string Status
    {
        get => PaymentStatus;
        set => PaymentStatus = value;
    }

    [NotMapped]
    public DateTime CreatedAt
    {
        get => DonationDate;
        set => DonationDate = value;
    }
}
