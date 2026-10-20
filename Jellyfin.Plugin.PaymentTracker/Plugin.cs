using System;
using System.Collections.Generic;
using Jellyfin.Plugin.PaymentTracker.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.PaymentTracker;

/// <summary>
/// Entry point of the payment tracker plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Fixed plugin ID. Must match the <c>guid</c> in manifest.json.
    /// </summary>
    public static readonly Guid PluginId = Guid.Parse("8c3d7f1e-5b2a-4e9d-9a61-2f4b7c0d1e35");

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override string Name => "Payment Tracker";

    /// <inheritdoc />
    public override Guid Id => PluginId;

    /// <inheritdoc />
    public override string Description => "Internal overview of user payments and grace periods.";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        var ns = GetType().Namespace;
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = $"{ns}.Configuration.configPage.html",
                EnableInMainMenu = true,
                MenuSection = "server",
                MenuIcon = "payments"
            },
            new PluginPageInfo
            {
                Name = "PaymentTrackerJs",
                EmbeddedResourcePath = $"{ns}.Configuration.configPage.js"
            },
            new PluginPageInfo
            {
                Name = "PaymentTrackerCss",
                EmbeddedResourcePath = $"{ns}.Configuration.configPage.css"
            }
        ];
    }
}
