using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request to set the paid-until month manually.
/// </summary>
public class SetMonthRequest : UserIdRequest
{
    /// <summary>Gets or sets the year (2000–9999).</summary>
    [Range(2000, 9999)]
    public int Year { get; set; }

    /// <summary>Gets or sets the month.</summary>
    [Range(1, 12)]
    public int Month { get; set; }
}
