using System;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// User including the calculated status, as sent to the frontend.
/// </summary>
public class UserPaymentDto
{
    /// <summary>Gets or sets the user ID.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the user name.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the year up to which the user has paid.</summary>
    public int PaidUntilYear { get; set; }

    /// <summary>Gets or sets the month up to which the user has paid.</summary>
    public int PaidUntilMonth { get; set; }

    /// <summary>Gets or sets a value indicating whether the grace period is active.</summary>
    public bool IsInGracePeriod { get; set; }

    /// <summary>Gets or sets the note.</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Gets or sets the calculated status.</summary>
    public PaymentStatus Status { get; set; }
}
