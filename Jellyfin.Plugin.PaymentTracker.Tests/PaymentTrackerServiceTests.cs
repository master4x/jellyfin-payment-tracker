using System;
using System.Linq;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Plugin.PaymentTracker.Configuration;
using Jellyfin.Plugin.PaymentTracker.Services;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;

namespace Jellyfin.Plugin.PaymentTracker.Tests;

public class PaymentTrackerServiceTests
{
    private readonly IUserManager userManager = Substitute.For<IUserManager>();
    private readonly InMemoryConfigStore configStore = new();

    // October 2026; FakeTimeProvider uses UTC as its local time zone.
    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void GetUsers_InitializesUnknownUsersWithCurrentMonthAndSavesOnce()
    {
        var alice = createUser("alice");
        var bob = createUser("bob");
        setJellyfinUsers(alice, bob);

        var result = createService().GetUsers();

        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.Equal((2026, 10), (u.PaidUntilYear, u.PaidUntilMonth)));
        Assert.Equal(
            new[] { alice.Id, bob.Id }.Order(),
            configStore.Configuration.Users.Select(s => s.UserId).Order());
        Assert.Equal(1, configStore.SaveCount);
    }

    [Fact]
    public void GetUsers_DoesNotSaveWithoutChanges()
    {
        var alice = createUser("alice");
        setJellyfinUsers(alice);
        addState(alice.Id, 2026, 12);

        createService().GetUsers();

        Assert.Equal(0, configStore.SaveCount);
    }

    [Fact]
    public void GetUsers_NeverReturnsUsersDeletedInJellyfin()
    {
        var alice = createUser("alice");
        setJellyfinUsers(alice);
        addState(alice.Id, 2026, 10);
        var deletedUserId = Guid.NewGuid();
        addState(deletedUserId, 2026, 10);

        var result = createService().GetUsers();

        Assert.DoesNotContain(result, u => u.UserId == deletedUserId);
        Assert.Equal(alice.Id, Assert.Single(result).UserId);
    }

    [Fact]
    public void GetUsers_SortsByNameIgnoringCase()
    {
        // Ordinal sorting would put "Bob" and "Charlie" before "alice".
        setJellyfinUsers(createUser("Charlie"), createUser("alice"), createUser("Bob"));

        var result = createService().GetUsers();

        Assert.Equal(new[] { "alice", "Bob", "Charlie" }, result.Select(u => u.UserName));
    }

    [Fact]
    public void AddMonth_RollsOverFromDecemberToJanuary()
    {
        var alice = createUser("alice");
        registerUser(alice);
        addState(alice.Id, 2026, 12);

        var result = createService().AddMonth(alice.Id);

        Assert.NotNull(result);
        Assert.Equal((2027, 1), (result.PaidUntilYear, result.PaidUntilMonth));
        Assert.Equal(1, configStore.SaveCount);
    }

    [Fact]
    public void UnknownUserId_ReturnsNullAndDoesNotSave()
    {
        var service = createService();
        var unknownId = Guid.NewGuid();

        Assert.Null(service.AddMonth(unknownId));
        Assert.Null(service.SetMonth(unknownId, 2027, 1));
        Assert.Null(service.ToggleGrace(unknownId));
        Assert.Null(service.UpdateNotes(unknownId, "note"));
        Assert.Equal(0, configStore.SaveCount);
    }

    [Theory]
    [InlineData("  paid in cash  ", "paid in cash")]
    [InlineData(null, "")]
    public void UpdateNotes_TrimsTextAndTreatsNullAsEmpty(string? notes, string expected)
    {
        var alice = createUser("alice");
        registerUser(alice);

        var result = createService().UpdateNotes(alice.Id, notes);

        Assert.NotNull(result);
        Assert.Equal(expected, result.Notes);
        Assert.Equal(expected, configStore.Configuration.Users.Single().Notes);
    }

    private static User createUser(string name) => new(name, "Default", "Default");

    private PaymentTrackerService createService()
        => new(userManager, configStore, NullLogger<PaymentTrackerService>.Instance, timeProvider);

    private void setJellyfinUsers(params User[] users)
    {
        userManager.GetUsers().Returns(users);
        foreach (var user in users)
        {
            registerUser(user);
        }
    }

    private void registerUser(User user) => userManager.GetUserById(user.Id).Returns(user);

    private void addState(Guid userId, int year, int month)
        => configStore.Configuration.Users.Add(new UserPaymentState { UserId = userId, PaidUntilYear = year, PaidUntilMonth = month });
}
