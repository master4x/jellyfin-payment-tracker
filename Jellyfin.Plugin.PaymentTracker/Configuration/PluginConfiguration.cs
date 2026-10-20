using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PaymentTracker.Configuration;

/// <summary>
/// Persistente Plugin-Konfiguration. Wird von Jellyfin als XML im
/// Ordner <c>plugins/configurations</c> abgelegt.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets den Zahlungsstatus aller bekannten Benutzer.
    /// </summary>
    /// <remarks>
    /// Muss eine öffentliche, setzbare <see cref="List{T}"/> sein, damit der
    /// XmlSerializer sie (de)serialisieren kann.
    /// </remarks>
#pragma warning disable CA2227 // Collection properties should be read only
    public List<UserPaymentState> Users { get; set; } = [];
#pragma warning restore CA2227
}

/// <summary>
/// Zahlungsstatus eines einzelnen Jellyfin-Benutzers.
/// </summary>
public class UserPaymentState
{
    /// <summary>
    /// Gets or sets die Jellyfin-User-ID (Guid im "N"-Format, ohne Bindestriche).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets das Jahr, bis zu dem bezahlt wurde (inklusive).
    /// </summary>
    public int PaidUntilYear { get; set; }

    /// <summary>
    /// Gets or sets den Monat (1–12), bis zu dem bezahlt wurde (inklusive).
    /// </summary>
    public int PaidUntilMonth { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether der Benutzer in einer Kulanz-/Testphase ist.
    /// </summary>
    public bool IsInGracePeriod { get; set; }

    /// <summary>
    /// Gets or sets eine kurze Freitext-Notiz.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
