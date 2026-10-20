using System;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request mit nur einer User-ID.
/// </summary>
public class UserIdRequest
{
    /// <summary>Gets or sets die User-ID. Fehlt sie im Request, antwortet die API mit 400.</summary>
    public required Guid UserId { get; set; }
}
