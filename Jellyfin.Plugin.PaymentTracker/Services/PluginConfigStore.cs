using System;
using Jellyfin.Plugin.PaymentTracker.Configuration;

namespace Jellyfin.Plugin.PaymentTracker.Services;

/// <summary>
/// Default <see cref="IPaymentConfigStore"/> backed by the plugin instance.
/// </summary>
public class PluginConfigStore : IPaymentConfigStore
{
    /// <inheritdoc />
    public PluginConfiguration Configuration => PluginInstance.Configuration;

    private static Plugin PluginInstance => Plugin.Instance
        ?? throw new InvalidOperationException("Payment tracker plugin is not initialized.");

    /// <inheritdoc />
    public void Save() => PluginInstance.SaveConfiguration();
}
