using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;
using TreeSharp;
using Trust.Data;
using Trust.Helpers;
using Trust.Localization;
using Trust.Logging;
using Trust.Managers;

namespace Trust;

/// <summary>
/// Main RebornBuddy plugin class for RB Trust.
/// </summary>
public class TrustPlugin : BotPlugin
{
    private Composite root;
    private DungeonManager dungeonManager;

    /// <inheritdoc/>
    public override string Author => "athlon";

    /// <inheritdoc/>
    public override string Name => Translations.PROJECT_NAME;

    /// <inheritdoc/>
    public override Version Version => new(7, 0, 04082024);

    /// <inheritdoc/>
    public override bool WantButton => false;

    /// <inheritdoc/>
    public override void OnInitialize()
    {
        PluginContainer plugin = PluginHelpers.GetSideStepPlugin();
        if (plugin != null)
        {
            plugin.Enabled = true;
        }

        root = new Decorator(c => CanTrust(), new ActionRunCoroutine(r => RunTrust()));
    }

    /// <inheritdoc/>
    public override void OnEnabled()
    {
        TreeRoot.OnStart += OnBotStart;
        TreeRoot.OnStop += OnBotStop;
        TreeHooks.Instance.OnHooksCleared += OnHooksCleared;

        if (TreeRoot.IsRunning)
        {
            AddHooks();
        }

        dungeonManager = new DungeonManager();
    }

    /// <inheritdoc/>
    public override void OnDisabled()
    {
        TreeRoot.OnStart -= OnBotStart;
        TreeRoot.OnStop -= OnBotStop;
        RemoveHooks();
    }

    /// <inheritdoc/>
    public override void OnShutdown()
    {
        OnDisabled();
    }

    /// <inheritdoc/>
    public override void OnButtonPress()
    {
        base.OnButtonPress();
    }

    private void AddHooks()
    {
        Logger.Information("Adding Trust Hook");
        TreeHooks.Instance.AddHook("TreeStart", root);
    }

    private void RemoveHooks()
    {
        Logger.Information("Removing Trust Hook");
        TreeHooks.Instance.RemoveHook("TreeStart", root);
    }

    private void OnBotStop(BotBase bot)
    {
        RemoveHooks();
    }

    private void OnBotStart(BotBase bot)
    {
        AddHooks();
    }

    private void OnHooksCleared(object sender, EventArgs e)
    {
        RemoveHooks();
    }

    private bool CanTrust()
    {
        if (LoadingHelpers.IsInInstance || WorldManager.ZoneId == (uint)ZoneId.UltimaThule)
        {
            return true;
        }

        return false;
    }

    private async Task<bool> RunTrust()
    {
        /*
        if (await TryRespawnPlayerAsync())
        {
            return true;
        }
        */

        await MovementHelpers.TryIncreaseMovementSpeedAsync();

        // LoggingHelpers.LogAllSpellCasts();
        LoggingHelpers.LogZoneChanges();

        return await dungeonManager.RunAsync();
    }

    private async Task<bool> TryRespawnPlayerAsync()
    {
        if (Core.Player.IsAlive)
        {
            return false;
        }

        if (!PartyManager.AllMembers.Any(pm => pm is TrustPartyMember))
        {
            return false;
        }

        Logger.Information(Translations.PLAYER_DIED_RELOADING_PROFILE);

        const int maxRespawnTime = 60_000;
        bool respawnedInReasonableTime = await Coroutine.Wait(maxRespawnTime, () => Core.Player.IsAlive);

        await LoadingHelpers.WaitForLoadingAsync();

        if (respawnedInReasonableTime)
        {
            NeoProfileManager.Load(CharacterSettings.Instance.LastNeoProfile, true);
            NeoProfileManager.UpdateCurrentProfileBehavior();
        }
        else
        {
            Logger.Error(Translations.PLAYER_FAILED_TO_RESPAWN, maxRespawnTime);
        }

        return true;
    }
}
