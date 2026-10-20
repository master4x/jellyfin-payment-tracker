using System.Collections.Generic;
using System.Net.Mime;
using Jellyfin.Plugin.PaymentTracker.Models;
using Jellyfin.Plugin.PaymentTracker.Services;
using MediaBrowser.Common.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.PaymentTracker.Api;

/// <summary>
/// Admin-API für den Payment Tracker.
/// </summary>
[ApiController]
[Authorize(Policy = Policies.RequiresElevation)]
[Route("PaymentTracker")]
[Produces(MediaTypeNames.Application.Json)]
public class PaymentController : ControllerBase
{
    private readonly PaymentTrackerService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentController"/> class.
    /// </summary>
    /// <param name="service">Payment-Tracker-Service.</param>
    public PaymentController(PaymentTrackerService service)
    {
        this.service = service;
    }

    /// <summary>
    /// Liefert alle Jellyfin-Benutzer mit berechnetem Zahlungsstatus.
    /// </summary>
    /// <returns>Liste aller Benutzer.</returns>
    [HttpGet("Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<UserPaymentDto>> GetUsers()
        => Ok(service.GetUsers());

    /// <summary>
    /// Verlängert den bezahlten Zeitraum um einen Monat.
    /// </summary>
    /// <param name="request">User-ID.</param>
    /// <returns>Der aktualisierte Benutzer.</returns>
    [HttpPost("AddMonth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> AddMonth([FromBody] UserIdRequest request)
        => okOrNotFound(service.AddMonth(request.UserId));

    /// <summary>
    /// Setzt den bezahlten Zeitraum manuell.
    /// </summary>
    /// <param name="request">User-ID, Jahr und Monat.</param>
    /// <returns>Der aktualisierte Benutzer.</returns>
    [HttpPost("SetMonth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> SetMonth([FromBody] SetMonthRequest request)
        => okOrNotFound(service.SetMonth(request.UserId, request.Year, request.Month));

    /// <summary>
    /// Schaltet den Kulanz-Status um.
    /// </summary>
    /// <param name="request">User-ID.</param>
    /// <returns>Der aktualisierte Benutzer.</returns>
    [HttpPost("ToggleGrace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> ToggleGrace([FromBody] UserIdRequest request)
        => okOrNotFound(service.ToggleGrace(request.UserId));

    /// <summary>
    /// Speichert die Notiz eines Benutzers.
    /// </summary>
    /// <param name="request">User-ID und Notiz.</param>
    /// <returns>Der aktualisierte Benutzer.</returns>
    [HttpPost("UpdateNotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> UpdateNotes([FromBody] UpdateNotesRequest request)
        => okOrNotFound(service.UpdateNotes(request.UserId, request.Notes));

    private ActionResult<UserPaymentDto> okOrNotFound(UserPaymentDto? dto)
        => dto is null ? NotFound("Benutzer nicht gefunden.") : Ok(dto);
}
