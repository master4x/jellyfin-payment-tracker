using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request zum Speichern einer Notiz.
/// </summary>
public class UpdateNotesRequest : UserIdRequest
{
    /// <summary>Gets or sets die Notiz.</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
