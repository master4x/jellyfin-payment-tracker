using System.ComponentModel.DataAnnotations;

namespace Jellyfin.Plugin.PaymentTracker.Models;

/// <summary>
/// Request to save a note.
/// </summary>
public class UpdateNotesRequest : UserIdRequest
{
    /// <summary>Gets or sets the note.</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
