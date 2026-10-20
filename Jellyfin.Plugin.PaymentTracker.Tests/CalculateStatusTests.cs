using Jellyfin.Plugin.PaymentTracker.Configuration;
using Jellyfin.Plugin.PaymentTracker.Models;
using Jellyfin.Plugin.PaymentTracker.Services;
using Xunit;

namespace Jellyfin.Plugin.PaymentTracker.Tests;

public class CalculateStatusTests
{
    [Theory]
    // Current month December 2026
    [InlineData(2027, 1, 2026, 12, PaymentStatus.Paid)]
    [InlineData(2026, 12, 2026, 12, PaymentStatus.Due)]
    [InlineData(2026, 11, 2026, 12, PaymentStatus.Overdue)]
    // Current month January 2027
    [InlineData(2027, 2, 2027, 1, PaymentStatus.Paid)]
    [InlineData(2027, 1, 2027, 1, PaymentStatus.Due)]
    [InlineData(2026, 12, 2027, 1, PaymentStatus.Overdue)]
    public void CalculatesStatusAcrossYearBoundary(int paidYear, int paidMonth, int currentYear, int currentMonth, PaymentStatus expected)
    {
        var state = new UserPaymentState { PaidUntilYear = paidYear, PaidUntilMonth = paidMonth };

        Assert.Equal(expected, PaymentTrackerService.CalculateStatus(state, currentYear, currentMonth));
    }

    [Theory]
    [InlineData(2027, 1)]  // would be Paid
    [InlineData(2026, 12)] // would be Due
    [InlineData(2020, 1)]  // would be Overdue
    public void GraceTakesPrecedenceOverAnyOtherStatus(int paidYear, int paidMonth)
    {
        var state = new UserPaymentState { PaidUntilYear = paidYear, PaidUntilMonth = paidMonth, IsInGracePeriod = true };

        Assert.Equal(PaymentStatus.Grace, PaymentTrackerService.CalculateStatus(state, 2026, 12));
    }

    [Theory]
    [InlineData(int.MaxValue, 12, 2026, 10, PaymentStatus.Paid)]
    [InlineData(2026, 10, int.MaxValue, 12, PaymentStatus.Overdue)]
    [InlineData(int.MaxValue, 12, int.MaxValue, 12, PaymentStatus.Due)]
    public void LargeYearsDoNotOverflow(int paidYear, int paidMonth, int currentYear, int currentMonth, PaymentStatus expected)
    {
        var state = new UserPaymentState { PaidUntilYear = paidYear, PaidUntilMonth = paidMonth };

        Assert.Equal(expected, PaymentTrackerService.CalculateStatus(state, currentYear, currentMonth));
    }
}
