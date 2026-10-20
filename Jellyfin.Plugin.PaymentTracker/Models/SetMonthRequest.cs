using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request zum manuellen Setzen des bezahlten Monats.
/// </summary>
public class SetMonthRequest : UserIdRequest
{
    /// <summary>Gets or sets das Jahr.</summary>
    [Range(2000, 2200)]
    public int Year { get; set; }

    /// <summary>Gets or sets den Monat.</summary>
    [Range(1, 12)]
    public int Month { get; set; }
}
