namespace GiveAid.Web.Services;

/// <summary>
/// Parameter object containing details required to process a simulated demo donation payment.
/// STRICT SECURITY: Real card numbers, CVVs, PINs, or banking credentials are NEVER accepted or stored.
/// </summary>
public class SimulatedPaymentRequest
{
    public int CauseId { get; set; }
    public decimal Amount { get; set; }
    public string? UserId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string DonorEmail { get; set; } = string.Empty;
    public string? DonorPhone { get; set; }
    public string RequestedOutcome { get; set; } = "Success"; // "Success" or "Failure"
    public string? SimulatedNotes { get; set; }
    public string IdempotencyToken { get; set; } = string.Empty;
}

/// <summary>
/// Result of a simulated demo payment transaction.
/// </summary>
public class SimulatedPaymentResult
{
    public bool IsSuccess { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
    public int DonationId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Service interface governing simulated payment processing for Give-AID demo workflows.
/// </summary>
public interface ISimulatedPaymentService
{
    Task<SimulatedPaymentResult> ProcessSimulatedPaymentAsync(SimulatedPaymentRequest request);
}
