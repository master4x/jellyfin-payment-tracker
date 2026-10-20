using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request mit nur einer User-ID.
/// </summary>
public class UserIdRequest
{
    /// <summary>Gets or sets die User-ID.</summary>
    [Required]
    public string UserId { get; set; } = string.Empty;
}
