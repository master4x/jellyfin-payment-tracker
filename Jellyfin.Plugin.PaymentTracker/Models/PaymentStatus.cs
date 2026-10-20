using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Calculated payment status. Serialized as a string (e.g. "Paid").
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PaymentStatus>))]
public enum PaymentStatus
{
    /// <summary>Paid in advance (PaidUntil &gt; current month).</summary>
    Paid,

    /// <summary>Due (PaidUntil == current month).</summary>
    Due,

    /// <summary>Overdue (PaidUntil &lt; current month).</summary>
    Overdue,

    /// <summary>Grace or trial period active.</summary>
    Grace
}
