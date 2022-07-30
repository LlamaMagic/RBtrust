using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using System;
using System.Threading.Tasks;
using TreeSharp;
using Trust.Dungeons;
using Trust.Helpers;
using Trust.Logging;

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

#if RB_CN
    /// <inheritdoc/>
    public override string Name => "亲信战友";
#else
    /// <inheritdoc/>
    public override string Name => "Trust";
#endif

    /// <inheritdoc/>
    public override Version Version => new(1, 2, 0);

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
        return LoadingHelpers.IsInInstance;
    }

    private async Task<bool> RunTrust()
    {
        if (!Core.Me.InCombat && MovementManager.IsMoving)
        {
            if (ActionManager.IsSprintReady)
            {
                ActionManager.Sprint();
                await Coroutine.Wait(1_000, () => !ActionManager.IsSprintReady);
            }
            else if (!Core.Player.HasAura(1199) && ActionManager.CanCast(7557, Core.Player))
            {
                ActionManager.DoAction(7557, Core.Player);
            }
        }

        if (await PlayerCheck())
        {
            return true;
        }

        // LoggingHelpers.LogAllSpellCasts();
        LoggingHelpers.LogZoneChanges();

        return await dungeonManager.RunAsync();
    }

    private async Task<bool> PlayerCheck()
    {
        if (Core.Me.CurrentHealthPercent <= 0)
        {
#if RB_CN
            Logger.Information($"检测到死亡");
#else
            Logger.Information($"Player has died.");
#endif
            await Coroutine.Sleep(10_000);
            NeoProfileManager.Load(NeoProfileManager.CurrentProfile.Path, true);
            NeoProfileManager.UpdateCurrentProfileBehavior();
            await Coroutine.Sleep(5_000);
            return true;
        }

        return false;
    }
}
