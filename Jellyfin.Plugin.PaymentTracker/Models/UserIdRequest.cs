using System;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request containing only a user ID.
/// </summary>
public class UserIdRequest
{
    /// <summary>Gets or sets the user ID. If it is missing, the API responds with 400.</summary>
    public required Guid UserId { get; set; }
}
