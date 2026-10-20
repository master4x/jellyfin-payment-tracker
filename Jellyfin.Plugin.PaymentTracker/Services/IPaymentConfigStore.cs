using Jellyfin.Plugin.PaymentTracker.Configuration;

namespace Jellyfin.Plugin.PaymentTracker.Services;

/// <summary>
/// Provides access to the persisted plugin configuration.
/// </summary>
public interface IPaymentConfigStore
{
    /// <summary>
    /// Gets the current plugin configuration.
    /// </summary>
    PluginConfiguration Configuration { get; }

    /// <summary>
    /// Persists the current plugin configuration.
    /// </summary>
    void Save();
}
