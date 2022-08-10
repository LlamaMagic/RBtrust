using Buddy.Coroutines;
using Clio.XmlEngine;
using ff14bot.Managers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Trust.Localization;

namespace ff14bot.NeoProfiles.Tags;

/// <summary>
/// Ensures Trust plugin is present and enabled.
/// </summary>
[XmlElement("CheckPlugins")]
public class CheckPluginsTag : AbstractTaskTag
{
    /// <inheritdoc/>
    protected override async Task<bool> RunAsync()
    {
        PluginContainer trustPlugin = PluginManager.Plugins.FirstOrDefault(p => p.Plugin.Name == Translations.PROJECT_NAME);

        if (trustPlugin != null)
        {
            // Plugin is installed and loaded correctly.  Force enable it so the user doesn't have to.
            trustPlugin.Enabled = true;

            string usabilityWarning = Translations.JOB_DIFFICULTY_WARNING;

            Core.OverlayManager.AddToast(
                () => usabilityWarning,
                TimeSpan.FromMilliseconds(5_000),
                System.Windows.Media.Color.FromRgb(29, 213, 226),
                System.Windows.Media.Color.FromRgb(14, 106, 113),
                new System.Windows.Media.FontFamily("Gautami"));

            await Coroutine.Sleep(6_000);
        }
        else
        {
            string pluginMissingError = Translations.TRUST_PLUGIN_MISSING;

            Core.OverlayManager.AddToast(
                () => pluginMissingError,
                TimeSpan.FromMilliseconds(5_000),
                System.Windows.Media.Color.FromRgb(210, 55, 65),
                System.Windows.Media.Color.FromRgb(105, 27, 32),
                new System.Windows.Media.FontFamily("Gautami"));

            await Coroutine.Sleep(6_000);

            TreeRoot.Stop(pluginMissingError);
        }

        return false;
    }
}
