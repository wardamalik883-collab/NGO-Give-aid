using System;
using System.Collections.Generic;

namespace GiveAid.Web.ViewModels;

/// <summary>
/// Dashboard summary view model delivering database metrics and recent operations to the Admin portal.
/// Implements Task 16: Admin Dashboard Integration requirements.
/// </summary>
public class AdminDashboardViewModel
{
    // High-level database summary counts
    public int TotalCauses { get; set; }
    public int ActiveCausesCount { get; set; }
    public int TotalDonations { get; set; }
    public decimal TotalDonationsAmount { get; set; }
    public int TotalMembers { get; set; }
    public int TotalNGOs { get; set; }
    public int TotalPartners { get; set; }
    public int TotalProgrammes { get; set; }
    public int ActiveProgrammesCount { get; set; }
    public int TotalGalleryItems { get; set; }
    public int TotalContactQueries { get; set; }
    public int PendingContactQueries { get; set; }
    public int PendingProgrammeRequests { get; set; }
    public int TotalInvitations { get; set; }

    // Quick review collections for recent activity
    public List<RecentDonationItemViewModel> RecentDonations { get; set; } = new();
    public List<RecentQueryItemViewModel> RecentQueries { get; set; } = new();
    public List<RecentInterestItemViewModel> RecentInterests { get; set; } = new();
}

public class RecentDonationItemViewModel
{
    public int Id { get; set; }
    public string TransactionReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string CauseTitle { get; set; } = string.Empty;
    public DateTime DonationDate { get; set; }
}

public class RecentQueryItemViewModel
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class RecentInterestItemViewModel
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ProgrammeTitle { get; set; } = string.Empty;
    public string InterestType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
}
