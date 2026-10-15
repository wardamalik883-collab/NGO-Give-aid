using System.ComponentModel.DataAnnotations;
using GiveAid.Web.Models;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// ViewModel for visitor inquiry submission on the public Contact Us page.
/// Implements FR-050 and FR-051.
/// </summary>
public class ContactFormViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Your Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    [Display(Name = "Contact Phone (Optional)")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please select or enter a subject.")]
    [StringLength(150, ErrorMessage = "Subject cannot exceed 150 characters.")]
    [Display(Name = "Inquiry Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please write your inquiry or message.")]
    [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters.")]
    [Display(Name = "Message / Inquiry")]
    public string Message { get; set; } = string.Empty;

    public bool IsAuthenticated { get; set; }
}

/// <summary>
/// ViewModel representing an individual FAQ entry.
/// </summary>
public class FaqItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for the public Help Centre and FAQ accordion page.
/// Implements FR-049.
/// </summary>
public class HelpIndexViewModel
{
    public List<FaqItemViewModel> Faqs { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string? ActiveCategory { get; set; }
}

/// <summary>
/// ViewModel for the admin queries listing screen with status filtering.
/// Implements FR-052 and FR-053.
/// </summary>
public class AdminQueryListViewModel
{
    public List<ContactQuery> Queries { get; set; } = new();
    public string? SelectedStatus { get; set; }
    public int TotalCount { get; set; }
    public int NewCount { get; set; }
    public int InProgressCount { get; set; }
    public int AnsweredCount { get; set; }
    public int ClosedCount { get; set; }
}

/// <summary>
/// ViewModel for reviewing a query and recording an administrator reply.
/// Implements FR-053.
/// </summary>
public class AdminQueryReplyViewModel
{
    public int QueryId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string? SenderPhone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string OriginalMessage { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string CurrentStatus { get; set; } = "New";
    public string? ExistingReply { get; set; }
    public DateTime? RepliedAt { get; set; }
    public string? UserId { get; set; }
    public string? RegisteredUserEmail { get; set; }

    [Required(ErrorMessage = "Please write a response to this inquiry.")]
    [StringLength(2000, ErrorMessage = "Reply message cannot exceed 2000 characters.")]
    [Display(Name = "Official Administrator Reply")]
    public string AdminReply { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Internal notes cannot exceed 1000 characters.")]
    [Display(Name = "Internal Operational Notes")]
    public string? AdminNotes { get; set; }

    [Display(Name = "Send Email Notification to Inquirer")]
    public bool SendEmailNotification { get; set; } = true;
}
