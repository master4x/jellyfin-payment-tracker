using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request zum manuellen Setzen des bezahlten Monats.
/// </summary>
public class SetMonthRequest : UserIdRequest
{
    /// <summary>Gets or sets das Jahr (2000–9999).</summary>
    [Range(2000, 9999)]
    public int Year { get; set; }

    /// <summary>Gets or sets den Monat.</summary>
    [Range(1, 12)]
    public int Month { get; set; }
}
