using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60: The Limitless Blue trial logic.
/// </summary>
public class LimitlessBlue : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.LimitlessBlue;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.LimitlessBlue;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Creating a small donut around Bismark's back so that casters are forced to move onto it's back
/*
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == 436,
            objectSelector: c => c.NpcId == EnemyNpc.ChitinCarapace && c.IsTargetable,
            outerRadius: 50.0f,
            innerRadius: 10.0F,
            priority: AvoidancePriority.Medium);

        // Creating a small donut around Bismark's back so that casters are forced to move onto it's back
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == 436,
            objectSelector: c => c.NpcId == EnemyNpc.Corona && c.IsTargetable,
            outerRadius: 50.0f,
            innerRadius: 10.0F,
            priority: AvoidancePriority.Medium);
*/

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        BattleCharacter Corona = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Corona)
            .LastOrDefault(bc => bc.IsVisible && bc.IsTargetable && bc.CurrentHealth > 0);
        BattleCharacter ChitinCarapace = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.ChitinCarapace)
            .LastOrDefault(bc => bc.IsVisible && bc.IsTargetable && bc.CurrentHealth > 0);
        var Whaleback = new Vector3(4.443342f, 0.1489766f, -6.865881f);

        if (Core.Me.InCombat && !Core.Me.HasAura(719) && ChitinCarapace != null)
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, $"Moving onto Whaleback");
            while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(Whaleback) > 1)
            {
                Navigator.PlayerMover.MoveTowards(Whaleback);
                await Coroutine.Yield();
            }

            await CommonTasks.StopMoving();
        }

        if (Core.Me.InCombat && !Core.Me.HasAura(719) && Corona != null)
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, $"Moving onto Whaleback");
            while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(Whaleback) > 1)
            {
                Navigator.PlayerMover.MoveTowards(Whaleback);
                await Coroutine.Yield();
            }

            await CommonTasks.StopMoving();
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Chitin Carapace
        /// </summary>
        public const uint ChitinCarapace = 3656;

        /// <summary>
        /// Boss: Corona
        /// </summary>
        public const uint Corona = 3657;
    }

    private static class ArenaCenter
    {
    }

    private static class EnemyAction
    {
    }

    private static class AblityTimers
    {
    }
}
