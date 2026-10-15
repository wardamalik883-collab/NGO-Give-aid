using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using GiveAid.Web.Models;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for submitting an expression of interest or volunteer application for a welfare programme.
/// </summary>
public class ProgrammeInterestFormViewModel
{
    [Required]
    public int ProgrammeId { get; set; }

    public string ProgrammeTitle { get; set; } = string.Empty;
    public string ProgrammeLocation { get; set; } = string.Empty;
    public DateTime ProgrammeDate { get; set; }

    [Required(ErrorMessage = "Please provide your full name.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Full Name")]
    public string ApplicantName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide your email address.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [Display(Name = "Email Address")]
    public string ApplicantEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide your contact phone number.")]
    [Phone(ErrorMessage = "Please provide a valid telephone number.")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    [Display(Name = "Phone Number")]
    public string ApplicantPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your preferred participation role.")]
    [StringLength(50)]
    [Display(Name = "Participation Role")]
    public string InterestType { get; set; } = "Volunteer"; // Volunteer or Participant

    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
    [Display(Name = "Relevant Experience, Skills, or Motivation")]
    public string? Message { get; set; }

    public List<SelectListItem> AvailableRoles { get; set; } = new()
    {
        new SelectListItem { Value = "Volunteer", Text = "Volunteer (Field Logistics, Registration, Support)" },
        new SelectListItem { Value = "Participant", Text = "Beneficiary / Community Participant" }
    };
}

/// <summary>
/// ViewModel for authenticated member's personal programme interest history.
/// </summary>
public class UserProgrammeInterestViewModel
{
    public List<ProgrammeInterest> Interests { get; set; } = new();
    public int TotalApplications { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public int RejectedCount { get; set; }
}

/// <summary>
/// ViewModel for administrator review and moderation of programme interest applications.
/// </summary>
public class AdminInterestListViewModel
{
    public List<ProgrammeInterest> Interests { get; set; } = new();
    public string? StatusFilter { get; set; }
    public int? ProgrammeFilter { get; set; }
    public string? SearchQuery { get; set; }
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public List<SelectListItem> ProgrammesList { get; set; } = new();
}
