namespace Jellyfin.Plugin.PaymentTracker.Models;

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
