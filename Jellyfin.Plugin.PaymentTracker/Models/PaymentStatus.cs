using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Berechneter Zahlungsstatus. Wird als String (z. B. "Paid") serialisiert.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PaymentStatus>))]
public enum PaymentStatus
{
    /// <summary>Im Voraus bezahlt (PaidUntil &gt; aktueller Monat).</summary>
    Paid,

    /// <summary>Fällig (PaidUntil == aktueller Monat).</summary>
    Due,

    /// <summary>Rückstand (PaidUntil &lt; aktueller Monat).</summary>
    Overdue,

    /// <summary>Kulanz-/Testphase aktiv.</summary>
    Grace
}
