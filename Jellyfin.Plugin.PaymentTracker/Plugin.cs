using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.PaymentTracker.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.PaymentTracker;

/// <summary>
/// Einstiegspunkt des Payment-Tracker-Plugins.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Feste Plugin-ID. Muss mit der <c>guid</c> in der manifest.json übereinstimmen.
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
    public override string Description => "Interne Zahlungs- und Kulanzübersicht der Benutzer.";

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
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", ns),
                EnableInMainMenu = true,
                MenuSection = "server",
                MenuIcon = "payments"
            },
            new PluginPageInfo
            {
                Name = "PaymentTrackerJs",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.js", ns)
            },
            new PluginPageInfo
            {
                Name = "PaymentTrackerCss",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.css", ns)
            }
        ];
    }
}
