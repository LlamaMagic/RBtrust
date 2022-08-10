using System.Globalization;
using Trust.Logging;

namespace Trust.Localization;

/// <summary>
/// Localization helpers.
/// </summary>
internal class LocalizationProvider
{
    /// <summary>
    /// Sets the user-facing language and number presentation for the BotBase UI and logging.
    ///
    /// Localized strings are loaded from Translations.{cultureCode}.resx files. Default strings are loaded from Translations.resx.
    /// </summary>
    /// <param name="cultureCode">Localization to display.</param>
    public void SetLocalization(string cultureCode)
    {
        CultureInfo culture;

        try
        {
            culture = CultureInfo.GetCultureInfo(cultureCode);
        }
        catch (CultureNotFoundException)
        {
            Logger.Error(Translations.LOCALIZATION_NOT_FOUND, cultureCode);
            return;
        }

        if (Translations.Culture == culture)
        {
            return;
        }

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        Translations.Culture = culture;

        Logger.Information(Translations.LOCALIZATION_CHANGED, cultureCode);
    }
}
