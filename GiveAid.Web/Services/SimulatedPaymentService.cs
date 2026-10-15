using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using GiveAid.Web.Data;
using GiveAid.Web.Models;

namespace GiveAid.Web.Services;

/// <summary>
/// Implements demo-only simulated payment processing for Give-AID.
/// Deterministic outcomes allow full testing of success and failure flows without real financial transactions.
/// STRICT RULE: Real credit cards, PINs, CVVs, or bank credentials are NEVER collected, stored, or processed.
/// </summary>
public class SimulatedPaymentService : ISimulatedPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SimulatedPaymentService> _logger;

    public SimulatedPaymentService(
        ApplicationDbContext context,
        IEmailService emailService,
        IMemoryCache cache,
        ILogger<SimulatedPaymentService> logger)
    {
        _context = context;
        _emailService = emailService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SimulatedPaymentResult> ProcessSimulatedPaymentAsync(SimulatedPaymentRequest request)
    {
        // 1. Basic Parameter Validation
        if (request.Amount <= 0)
        {
            return new SimulatedPaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "Donation amount must be greater than zero."
            };
        }

        if (request.Amount > 10000000m)
        {
            return new SimulatedPaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "Donation amount cannot exceed PKR 10,000,000 per transaction."
            };
        }

        // 2. Anti-duplicate submission check (Idempotency)
        if (!string.IsNullOrWhiteSpace(request.IdempotencyToken))
        {
            var cacheKey = $"idempotency_donation_{request.IdempotencyToken}";
            if (_cache.TryGetValue(cacheKey, out SimulatedPaymentResult? cachedResult) && cachedResult != null)
            {
                _logger.LogWarning("Duplicate donation attempt blocked via idempotency token: {Token}", request.IdempotencyToken);
                return cachedResult;
            }
        }

        // 3. Validate Target Cause in Database
        var cause = await _context.Causes
            .Include(c => c.Ngo)
            .FirstOrDefaultAsync(c => c.Id == request.CauseId && c.IsActive);

        if (cause == null)
        {
            return new SimulatedPaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "The selected charitable cause does not exist or is currently inactive."
            };
        }

        // 4. Generate Non-Sensitive Unique Transaction Reference
        var reference = await GenerateUniqueReferenceAsync();

        // 5. Evaluate Deterministic Demo Outcome
        bool isSuccessRequested = !string.Equals(request.RequestedOutcome, "Failure", StringComparison.OrdinalIgnoreCase);

        var donation = new Donation
        {
            TransactionReference = reference,
            Amount = request.Amount,
            DonationDate = DateTime.UtcNow,
            DonorName = request.DonorName.Trim(),
            DonorEmail = request.DonorEmail.Trim(),
            DonorPhone = string.IsNullOrWhiteSpace(request.DonorPhone) ? null : request.DonorPhone.Trim(),
            PaymentStatus = isSuccessRequested ? "Completed" : "Failed",
            IsSimulated = true,
            CauseId = cause.Id,
            UserId = string.IsNullOrWhiteSpace(request.UserId) ? null : request.UserId,
            PaymentNotes = isSuccessRequested 
                ? "Simulated demo payment completed successfully. No real money transferred."
                : (request.SimulatedNotes ?? "Simulated payment test outcome: Declined by simulated payment network.")
        };

        // 6. If Successful, Update Cause Funding Progress
        if (isSuccessRequested)
        {
            cause.CurrentAmount += request.Amount;
        }

        _context.Donations.Add(donation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Recorded simulated donation {Reference} for Cause {CauseId} ({Status}, Amount: {Amount})",
            donation.TransactionReference, cause.Id, donation.PaymentStatus, donation.Amount);

        // 7. Send Simulated Email Receipt (Non-blocking fallback)
        if (isSuccessRequested)
        {
            try
            {
                var emailSubject = $"Give-AID Demo Donation Receipt [{donation.TransactionReference}]";
                var emailBody = $@"
                    <h3>Thank you for your demo contribution, {donation.DonorName}!</h3>
                    <p>This email confirms your simulated donation to <strong>{cause.Title}</strong>.</p>
                    <table style='border-collapse:collapse;width:100%;max-width:500px;'>
                        <tr><td style='padding:6px;border:1px solid #ddd;'><strong>Transaction Reference:</strong></td><td style='padding:6px;border:1px solid #ddd;'>{donation.TransactionReference}</td></tr>
                        <tr><td style='padding:6px;border:1px solid #ddd;'><strong>Amount:</strong></td><td style='padding:6px;border:1px solid #ddd;'>PKR {donation.Amount:N2}</td></tr>
                        <tr><td style='padding:6px;border:1px solid #ddd;'><strong>Date:</strong></td><td style='padding:6px;border:1px solid #ddd;'>{donation.DonationDate:dd MMM yyyy, HH:mm UTC}</td></tr>
                        <tr><td style='padding:6px;border:1px solid #ddd;'><strong>Status:</strong></td><td style='padding:6px;border:1px solid #ddd;'>{donation.PaymentStatus}</td></tr>
                    </table>
                    <p style='color:#666;font-size:12px;margin-top:15px;'>
                        <em>Notice: This was a simulated demo payment. No real currency or payment credentials were used or charged.</em>
                    </p>";

                await _emailService.SendEmailAsync(donation.DonorEmail, emailSubject, emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to dispatch simulated receipt email for donation {Reference}", donation.TransactionReference);
            }
        }

        var result = new SimulatedPaymentResult
        {
            IsSuccess = isSuccessRequested,
            Status = donation.PaymentStatus,
            TransactionReference = donation.TransactionReference,
            DonationId = donation.Id,
            ErrorMessage = isSuccessRequested ? null : "Simulated demo payment failed as requested for testing."
        };

        // Cache idempotency token for 10 minutes to prevent accidental duplicate submission
        if (!string.IsNullOrWhiteSpace(request.IdempotencyToken))
        {
            var cacheKey = $"idempotency_donation_{request.IdempotencyToken}";
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
        }

        return result;
    }

    private async Task<string> GenerateUniqueReferenceAsync()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomBytes = RandomNumberGenerator.GetBytes(3);
            var randomPart = Convert.ToHexString(randomBytes);
            var reference = $"GIVEAID-DON-{datePart}-{randomPart}";

            bool exists = await _context.Donations.AnyAsync(d => d.TransactionReference == reference);
            if (!exists)
            {
                return reference;
            }
        }

        return $"GIVEAID-DON-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..30].ToUpper();
    }
}
