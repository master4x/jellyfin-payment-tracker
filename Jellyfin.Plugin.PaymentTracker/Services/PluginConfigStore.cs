using System;
using Jellyfin.Plugin.PaymentTracker.Configuration;

namespace Jellyfin.Plugin.PaymentTracker.Services;

/// <summary>
/// Default <see cref="IPaymentConfigStore"/> backed by the plugin instance.
/// </summary>
public class PluginConfigStore : IPaymentConfigStore
{
    /// <inheritdoc />
    public PluginConfiguration Configuration => pluginInstance.Configuration;

    private static Plugin pluginInstance => Plugin.Instance
        ?? throw new InvalidOperationException("Payment tracker plugin is not initialized.");

    /// <inheritdoc />
    public void Save() => pluginInstance.SaveConfiguration();
}
