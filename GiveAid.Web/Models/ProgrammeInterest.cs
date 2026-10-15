using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAid.Web.Models;

/// <summary>
/// Represents an application or expression of interest to participate or volunteer in a community welfare programme.
/// Implements Task 14: Programme Interest / Participation requirements.
/// </summary>
public class ProgrammeInterest
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a programme.")]
    [Display(Name = "Welfare Programme")]
    public int ProgrammeId { get; set; }

    [ForeignKey("ProgrammeId")]
    public Programme? Programme { get; set; }

    // Associated member account (nullable for guest applicants)
    [StringLength(450)]
    [Display(Name = "Registered User ID")]
    public string? UserId { get; set; }

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    [Required(ErrorMessage = "Please provide your full name.")]
    [StringLength(100, ErrorMessage = "Applicant name cannot exceed 100 characters.")]
    [Display(Name = "Applicant Full Name")]
    public string ApplicantName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [Display(Name = "Applicant Email Address")]
    public string ApplicantEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide your contact phone number.")]
    [Phone(ErrorMessage = "Please provide a valid telephone number.")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    [Display(Name = "Contact Telephone")]
    public string ApplicantPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your participation role.")]
    [StringLength(50)]
    [Display(Name = "Interest Type")]
    public string InterestType { get; set; } = "Volunteer"; // Volunteer or Participant

    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
    [Display(Name = "Message or Reason for Applying")]
    public string? Message { get; set; }

    [StringLength(500)]
    [Display(Name = "Volunteer Skills or Notes")]
    public string? VolunteerNote { get; set; }

    [Required]
    [StringLength(25)]
    [Display(Name = "Application Status")]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    [Display(Name = "Submission Date (UTC)")]
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    [Display(Name = "Administrator Review Notes")]
    public string? AdminNotes { get; set; }

    [Display(Name = "Reviewed Date (UTC)")]
    public DateTime? ReviewedAt { get; set; }

    // Property aliases for roadmap conventions
    [NotMapped]
    public DateTime CreatedAt
    {
        get => SubmissionDate;
        set => SubmissionDate = value;
    }
}
