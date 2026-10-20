using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PaymentTracker.Configuration;

/// <summary>
/// Persisted plugin configuration. Stored by Jellyfin as XML in the
/// <c>plugins/configurations</c> folder.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the payment state of all known users.
    /// </summary>
    /// <remarks>
    /// Must be a public, settable <see cref="List{T}"/> so the
    /// XmlSerializer can (de)serialize it.
    /// </remarks>
#pragma warning disable CA2227 // Collection properties should be read only
    public List<UserPaymentState> Users { get; set; } = [];
#pragma warning restore CA2227
}

/// <summary>
/// Payment state of a single Jellyfin user.
/// </summary>
public class UserPaymentState
{
    /// <summary>
    /// Gets or sets the Jellyfin user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the year up to which the user has paid (inclusive).
    /// </summary>
    public int PaidUntilYear { get; set; }

    /// <summary>
    /// Gets or sets the month (1–12) up to which the user has paid (inclusive).
    /// </summary>
    public int PaidUntilMonth { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is in a grace or trial period.
    /// </summary>
    public bool IsInGracePeriod { get; set; }

    /// <summary>
    /// Gets or sets a short free-text note.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
