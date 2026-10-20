using Jellyfin.Plugin.PaymentTracker.Configuration;
using Jellyfin.Plugin.PaymentTracker.Services;

namespace Jellyfin.Plugin.PaymentTracker.Tests;

/// <summary>
/// In-memory <see cref="IPaymentConfigStore"/> that counts how often it was saved.
/// </summary>
public class InMemoryConfigStore : IPaymentConfigStore
{
    /// <inheritdoc />
    public PluginConfiguration Configuration { get; } = new();

    /// <summary>
    /// Gets the number of <see cref="Save"/> calls.
    /// </summary>
    public int SaveCount { get; private set; }

    /// <inheritdoc />
    public void Save() => SaveCount++;
}
