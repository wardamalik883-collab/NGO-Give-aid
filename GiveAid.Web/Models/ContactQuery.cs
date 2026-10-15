using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiveAid.Web.Models;

/// <summary>
/// Represents a visitor or member inquiry submitted through the Contact Us form.
/// Implements FR-050, FR-051, FR-052, FR-053, and FR-054.
/// </summary>
public class ContactQuery
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Your full name is required.")]
    [StringLength(100, ErrorMessage = "Sender name cannot exceed 100 characters.")]
    [Display(Name = "Sender Full Name")]
    public string SenderName { get; set; } = string.Empty;

    /// <summary>Alias for SenderName to conform to Task 12 specification.</summary>
    [NotMapped]
    public string Name
    {
        get => SenderName;
        set => SenderName = value;
    }

    [Required(ErrorMessage = "A valid email address is required.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [Display(Name = "Sender Email Address")]
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>Alias for SenderEmail to conform to Task 12 specification.</summary>
    [NotMapped]
    public string Email
    {
        get => SenderEmail;
        set => SenderEmail = value;
    }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    [Display(Name = "Sender Phone Number")]
    public string? SenderPhone { get; set; }

    /// <summary>Alias for SenderPhone.</summary>
    [NotMapped]
    public string? Phone
    {
        get => SenderPhone;
        set => SenderPhone = value;
    }

    [Required(ErrorMessage = "Inquiry subject is required.")]
    [StringLength(150, ErrorMessage = "Subject cannot exceed 150 characters.")]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide your inquiry message.")]
    [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Submitted Timestamp (UTC)")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Alias for SubmittedAt to conform to Task 12 specification.</summary>
    [NotMapped]
    public DateTime CreatedAt
    {
        get => SubmittedAt;
        set => SubmittedAt = value;
    }

    [Required]
    [StringLength(25)]
    [Display(Name = "Resolution Status")]
    public string Status { get; set; } = "New"; // New, In Progress, Answered, Closed

    [StringLength(1000)]
    [Display(Name = "Internal Administrator Notes")]
    public string? AdminNotes { get; set; }

    [StringLength(2000, ErrorMessage = "Admin reply cannot exceed 2000 characters.")]
    [Display(Name = "Administrator Reply")]
    public string? AdminReply { get; set; }

    [Display(Name = "Replied Timestamp (UTC)")]
    public DateTime? RepliedAt { get; set; }

    // Optional link if inquiry was submitted by a logged-in member (FR-054)
    [StringLength(450)]
    [Display(Name = "Registered User ID")]
    public string? UserId { get; set; }

    /// <summary>Navigation property for registered user account (optional).</summary>
    public virtual ApplicationUser? User { get; set; }
}
