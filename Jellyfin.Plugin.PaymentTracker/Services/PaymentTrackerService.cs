using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.PaymentTracker.Configuration;
using Jellyfin.Plugin.PaymentTracker.Models;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PaymentTracker.Services;

/// <summary>
/// Encapsulates the sync with <see cref="IUserManager"/>, the due-date logic
/// and all changes to the plugin configuration.
/// </summary>
/// <param name="userManager">Jellyfin user manager.</param>
/// <param name="logger">Logger.</param>
/// <param name="timeProvider">Time source (replaceable for tests); defaults to the system clock.</param>
public class PaymentTrackerService(
    IUserManager userManager,
    ILogger<PaymentTrackerService> logger,
    TimeProvider? timeProvider = null)
{
    // All reads and writes of the configuration go through this lock,
    // otherwise concurrent API calls could overwrite each other's changes to the XML file.
    private static readonly object syncRoot = new();

    private readonly TimeProvider timeProvider = timeProvider ?? TimeProvider.System;

    private static Plugin PluginInstance => Plugin.Instance
        ?? throw new InvalidOperationException("Payment tracker plugin is not initialized.");

    /// <summary>
    /// Returns all existing Jellyfin users including their calculated payment status.
    /// Unknown users are initialized with the current month.
    /// </summary>
    /// <returns>List of users, sorted alphabetically.</returns>
    public IReadOnlyList<UserPaymentDto> GetUsers()
    {
        lock (syncRoot)
        {
            var config = PluginInstance.Configuration;
            var (curYear, curMonth) = getCurrentYearAndMonth();
            var states = config.Users.ToDictionary(s => s.UserId);
            var changed = false;
            var result = new List<UserPaymentDto>();

            // Jellyfin 12: GetUsers() replaces the former IUserManager.Users property.
            foreach (var user in userManager.GetUsers())
            {
                if (!states.TryGetValue(user.Id, out var state))
                {
                    state = new UserPaymentState
                    {
                        UserId = user.Id,
                        PaidUntilYear = curYear,
                        PaidUntilMonth = curMonth
                    };
                    config.Users.Add(state);
                    states[user.Id] = state;
                    changed = true;
                    logger.LogInformation("PaymentTracker: Initialized new user {UserName} with {Month}/{Year}", user.Username, curMonth, curYear);
                }

                result.Add(createUserPaymentDto(state, user.Username, (curYear, curMonth)));
            }

            // Entries of deleted users are intentionally kept in the configuration
            // (no data loss) but are no longer returned.
            if (changed)
            {
                PluginInstance.SaveConfiguration();
            }

            return result
                .OrderBy(u => u.UserName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
    }

    /// <summary>
    /// Extends the paid period by one month (including year rollover).
    /// </summary>
    /// <param name="userId">Jellyfin user ID.</param>
    /// <returns>The updated user, or <c>null</c> if unknown.</returns>
    public UserPaymentDto? AddMonth(Guid userId)
        => updateAndSaveUserState(userId, state =>
        {
            if (state.PaidUntilMonth >= 12)
            {
                state.PaidUntilMonth = 1;
                state.PaidUntilYear++;
            }
            else
            {
                state.PaidUntilMonth++;
            }
        });

    /// <summary>
    /// Sets the paid period to a specific month.
    /// </summary>
    /// <param name="userId">Jellyfin user ID.</param>
    /// <param name="year">Year.</param>
    /// <param name="month">Month (1–12).</param>
    /// <returns>The updated user, or <c>null</c> if unknown.</returns>
    public UserPaymentDto? SetMonth(Guid userId, int year, int month)
        => updateAndSaveUserState(userId, state =>
        {
            state.PaidUntilYear = year;
            state.PaidUntilMonth = month;
        });

    /// <summary>
    /// Toggles the grace period.
    /// </summary>
    /// <param name="userId">Jellyfin user ID.</param>
    /// <returns>The updated user, or <c>null</c> if unknown.</returns>
    public UserPaymentDto? ToggleGrace(Guid userId)
        => updateAndSaveUserState(userId, state => state.IsInGracePeriod = !state.IsInGracePeriod);

    /// <summary>
    /// Saves a user's note.
    /// </summary>
    /// <param name="userId">Jellyfin user ID.</param>
    /// <param name="notes">Free text.</param>
    /// <returns>The updated user, or <c>null</c> if unknown.</returns>
    public UserPaymentDto? UpdateNotes(Guid userId, string? notes)
        => updateAndSaveUserState(userId, state => state.Notes = (notes ?? string.Empty).Trim());

    /// <summary>
    /// Calculates the status based on the current calendar month.
    /// </summary>
    /// <param name="state">Stored state.</param>
    /// <param name="currentYear">Current year.</param>
    /// <param name="currentMonth">Current month.</param>
    /// <returns>The calculated status.</returns>
    public static PaymentStatus CalculateStatus(UserPaymentState state, int currentYear, int currentMonth)
    {
        if (state.IsInGracePeriod)
        {
            return PaymentStatus.Grace;
        }

        // Tuples compare lexicographically (year first, then month) – no overflow for large years.
        return (state.PaidUntilYear, state.PaidUntilMonth).CompareTo((currentYear, currentMonth)) switch
        {
            > 0 => PaymentStatus.Paid,
            0 => PaymentStatus.Due,
            _ => PaymentStatus.Overdue
        };
    }

    private (int Year, int Month) getCurrentYearAndMonth()
    {
        var now = timeProvider.GetLocalNow();
        return (now.Year, now.Month);
    }

    private UserPaymentDto? updateAndSaveUserState(Guid userId, Action<UserPaymentState> change)
    {
        var user = userManager.GetUserById(userId);
        if (user is null)
        {
            return null;
        }

        lock (syncRoot)
        {
            var config = PluginInstance.Configuration;
            var (curYear, curMonth) = getCurrentYearAndMonth();
            var state = config.Users.FirstOrDefault(s => s.UserId == userId);
            if (state is null)
            {
                state = new UserPaymentState { UserId = userId, PaidUntilYear = curYear, PaidUntilMonth = curMonth };
                config.Users.Add(state);
            }

            change(state);
            PluginInstance.SaveConfiguration();
            return createUserPaymentDto(state, user.Username, (curYear, curMonth));
        }
    }

    private UserPaymentDto createUserPaymentDto(UserPaymentState state, string userName, (int Year, int Month) current)
    {
        return new UserPaymentDto
        {
            UserId = state.UserId,
            UserName = userName,
            PaidUntilYear = state.PaidUntilYear,
            PaidUntilMonth = state.PaidUntilMonth,
            IsInGracePeriod = state.IsInGracePeriod,
            Notes = state.Notes,
            Status = CalculateStatus(state, current.Year, current.Month)
        };
    }
}
