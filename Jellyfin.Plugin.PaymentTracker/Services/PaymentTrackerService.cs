using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Plugin.PaymentTracker.Configuration;
using Jellyfin.Plugin.PaymentTracker.Models;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PaymentTracker.Services;

/// <summary>
/// Kapselt Synchronisation mit dem <see cref="IUserManager"/>, die Stichtag-Logik
/// und alle Änderungen an der Plugin-Konfiguration.
/// </summary>
public class PaymentTrackerService
{
    // Alle Lese-/Schreibzugriffe auf die Konfiguration laufen über diesen Lock,
    // da parallele API-Aufrufe sonst die XML-Datei gegenseitig überschreiben könnten.
    private static readonly object syncRoot = new();

    private readonly IUserManager userManager;
    private readonly ILogger<PaymentTrackerService> logger;
    private readonly TimeProvider timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentTrackerService"/> class.
    /// </summary>
    /// <param name="userManager">Jellyfin-Benutzerverwaltung.</param>
    /// <param name="logger">Logger.</param>
    public PaymentTrackerService(IUserManager userManager, ILogger<PaymentTrackerService> logger)
        : this(userManager, logger, TimeProvider.System)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentTrackerService"/> class.
    /// </summary>
    /// <param name="userManager">Jellyfin-Benutzerverwaltung.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="timeProvider">Zeitquelle (für Tests austauschbar).</param>
    public PaymentTrackerService(IUserManager userManager, ILogger<PaymentTrackerService> logger, TimeProvider timeProvider)
    {
        this.userManager = userManager;
        this.logger = logger;
        this.timeProvider = timeProvider;
    }

    private static Plugin PluginInstance => Plugin.Instance
        ?? throw new InvalidOperationException("Payment-Tracker-Plugin ist nicht initialisiert.");

    /// <summary>
    /// Liefert alle existierenden Jellyfin-Benutzer inkl. berechnetem Zahlungsstatus.
    /// Unbekannte Benutzer werden dabei mit dem aktuellen Monat angelegt.
    /// </summary>
    /// <returns>Liste der Benutzer, alphabetisch sortiert.</returns>
    public IReadOnlyList<UserPaymentDto> GetUsers()
    {
        lock (syncRoot)
        {
            var config = PluginInstance.Configuration;
            var (curYear, curMonth) = CurrentMonth();
            var states = config.Users.ToDictionary(s => s.UserId, StringComparer.OrdinalIgnoreCase);
            var changed = false;
            var result = new List<UserPaymentDto>();

            // Jellyfin 12: GetUsers() ersetzt die frühere Property IUserManager.Users.
            foreach (var user in userManager.GetUsers())
            {
                var id = user.Id.ToString("N");
                if (!states.TryGetValue(id, out var state))
                {
                    state = new UserPaymentState
                    {
                        UserId = id,
                        PaidUntilYear = curYear,
                        PaidUntilMonth = curMonth
                    };
                    config.Users.Add(state);
                    states[id] = state;
                    changed = true;
                    logger.LogInformation("PaymentTracker: Neuer Benutzer {UserName} mit {Month}/{Year} initialisiert", user.Username, curMonth, curYear);
                }

                result.Add(ToDto(state, user.Username));
            }

            // Einträge gelöschter Benutzer bleiben bewusst in der Konfiguration erhalten
            // (kein Datenverlust), werden aber nicht mehr ausgeliefert.
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
    /// Verlängert den bezahlten Zeitraum um einen Monat (mit Jahreswechsel).
    /// </summary>
    /// <param name="userId">Jellyfin-User-ID.</param>
    /// <returns>Der aktualisierte Benutzer oder <c>null</c>, falls unbekannt.</returns>
    public UserPaymentDto? AddMonth(string userId)
        => Mutate(userId, state =>
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
    /// Setzt den bezahlten Zeitraum auf einen konkreten Monat.
    /// </summary>
    /// <param name="userId">Jellyfin-User-ID.</param>
    /// <param name="year">Jahr.</param>
    /// <param name="month">Monat (1–12).</param>
    /// <returns>Der aktualisierte Benutzer oder <c>null</c>, falls unbekannt.</returns>
    public UserPaymentDto? SetMonth(string userId, int year, int month)
        => Mutate(userId, state =>
        {
            state.PaidUntilYear = year;
            state.PaidUntilMonth = month;
        });

    /// <summary>
    /// Schaltet den Kulanz-Status um.
    /// </summary>
    /// <param name="userId">Jellyfin-User-ID.</param>
    /// <returns>Der aktualisierte Benutzer oder <c>null</c>, falls unbekannt.</returns>
    public UserPaymentDto? ToggleGrace(string userId)
        => Mutate(userId, state => state.IsInGracePeriod = !state.IsInGracePeriod);

    /// <summary>
    /// Speichert die Notiz eines Benutzers.
    /// </summary>
    /// <param name="userId">Jellyfin-User-ID.</param>
    /// <param name="notes">Freitext.</param>
    /// <returns>Der aktualisierte Benutzer oder <c>null</c>, falls unbekannt.</returns>
    public UserPaymentDto? UpdateNotes(string userId, string? notes)
        => Mutate(userId, state => state.Notes = (notes ?? string.Empty).Trim());

    /// <summary>
    /// Berechnet den Status anhand des aktuellen Kalendermonats.
    /// </summary>
    /// <param name="state">Gespeicherter Zustand.</param>
    /// <param name="currentYear">Aktuelles Jahr.</param>
    /// <param name="currentMonth">Aktueller Monat.</param>
    /// <returns>Der berechnete Status.</returns>
    public static PaymentStatus CalculateStatus(UserPaymentState state, int currentYear, int currentMonth)
    {
        if (state.IsInGracePeriod)
        {
            return PaymentStatus.Grace;
        }

        // Monate als fortlaufenden Index vergleichen – spart Sonderfälle beim Jahreswechsel.
        var paid = (state.PaidUntilYear * 12) + (state.PaidUntilMonth - 1);
        var current = (currentYear * 12) + (currentMonth - 1);

        return paid.CompareTo(current) switch
        {
            > 0 => PaymentStatus.Paid,
            0 => PaymentStatus.Due,
            _ => PaymentStatus.Overdue
        };
    }

    private (int Year, int Month) CurrentMonth()
    {
        var now = timeProvider.GetLocalNow();
        return (now.Year, now.Month);
    }

    private UserPaymentDto? Mutate(string userId, Action<UserPaymentState> change)
    {
        if (!Guid.TryParse(userId, out var guid))
        {
            return null;
        }

        var user = userManager.GetUserById(guid);
        if (user is null)
        {
            return null;
        }

        lock (syncRoot)
        {
            var config = PluginInstance.Configuration;
            var id = guid.ToString("N");
            var state = config.Users.FirstOrDefault(s => string.Equals(s.UserId, id, StringComparison.OrdinalIgnoreCase));
            if (state is null)
            {
                var (y, m) = CurrentMonth();
                state = new UserPaymentState { UserId = id, PaidUntilYear = y, PaidUntilMonth = m };
                config.Users.Add(state);
            }

            change(state);
            PluginInstance.SaveConfiguration();
            return ToDto(state, user.Username);
        }
    }

    private UserPaymentDto ToDto(UserPaymentState state, string userName)
    {
        var (y, m) = CurrentMonth();
        return new UserPaymentDto
        {
            UserId = state.UserId,
            UserName = userName,
            PaidUntilYear = state.PaidUntilYear,
            PaidUntilMonth = state.PaidUntilMonth,
            IsInGracePeriod = state.IsInGracePeriod,
            Notes = state.Notes,
            Status = CalculateStatus(state, y, m)
        };
    }
}
