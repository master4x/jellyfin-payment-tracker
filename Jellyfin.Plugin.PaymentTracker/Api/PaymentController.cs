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
/// Admin API of the payment tracker.
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
    /// <param name="service">Payment tracker service.</param>
    public PaymentController(PaymentTrackerService service)
    {
        this.service = service;
    }

    /// <summary>
    /// Returns all Jellyfin users with their calculated payment status.
    /// </summary>
    /// <returns>List of all users.</returns>
    [HttpGet("Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<UserPaymentDto>> GetUsers()
        => Ok(service.GetUsers());

    /// <summary>
    /// Extends the paid period by one month.
    /// </summary>
    /// <param name="request">User ID.</param>
    /// <returns>The updated user.</returns>
    [HttpPost("AddMonth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> AddMonth([FromBody] UserIdRequest request)
        => getActionResult(service.AddMonth(request.UserId));

    /// <summary>
    /// Sets the paid period manually.
    /// </summary>
    /// <param name="request">User ID, year and month.</param>
    /// <returns>The updated user.</returns>
    [HttpPost("SetMonth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> SetMonth([FromBody] SetMonthRequest request)
        => getActionResult(service.SetMonth(request.UserId, request.Year, request.Month));

    /// <summary>
    /// Toggles the grace period.
    /// </summary>
    /// <param name="request">User ID.</param>
    /// <returns>The updated user.</returns>
    [HttpPost("ToggleGrace")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> ToggleGrace([FromBody] UserIdRequest request)
        => getActionResult(service.ToggleGrace(request.UserId));

    /// <summary>
    /// Saves a user's note.
    /// </summary>
    /// <param name="request">User ID and note.</param>
    /// <returns>The updated user.</returns>
    [HttpPost("UpdateNotes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserPaymentDto> UpdateNotes([FromBody] UpdateNotesRequest request)
        => getActionResult(service.UpdateNotes(request.UserId, request.Notes));

    private ActionResult<UserPaymentDto> getActionResult(UserPaymentDto? dto)
        => dto is null ? NotFound("User not found.") : Ok(dto);
}
