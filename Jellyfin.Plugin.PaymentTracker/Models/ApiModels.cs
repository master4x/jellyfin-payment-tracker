using System.ComponentModel.DataAnnotations;
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

/// <summary>
/// Benutzer inkl. berechnetem Status, wie er an das Frontend geht.
/// </summary>
public class UserPaymentDto
{
    /// <summary>Gets or sets die User-ID.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets den Benutzernamen.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets das Jahr, bis zu dem bezahlt ist.</summary>
    public int PaidUntilYear { get; set; }

    /// <summary>Gets or sets den Monat, bis zu dem bezahlt ist.</summary>
    public int PaidUntilMonth { get; set; }

    /// <summary>Gets or sets a value indicating whether Kulanz aktiv ist.</summary>
    public bool IsInGracePeriod { get; set; }

    /// <summary>Gets or sets die Notiz.</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Gets or sets den berechneten Status.</summary>
    public PaymentStatus Status { get; set; }
}

/// <summary>
/// Request mit nur einer User-ID.
/// </summary>
public class UserIdRequest
{
    /// <summary>Gets or sets die User-ID.</summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
}

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

/// <summary>
/// Request zum Speichern einer Notiz.
/// </summary>
public class UpdateNotesRequest : UserIdRequest
{
    /// <summary>Gets or sets die Notiz.</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
