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
public class ContainmentBayS1T7 : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.ContainmentBayS1T7;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ContainmentBayS1T7;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Avoid wind circle
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayS1T7,
            objectSelector: bc => bc.NpcId == EnemyNpc.CoronalWind && bc.IsVisible,
            radiusProducer: bc => 5.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayS1T7,
            () => ArenaCenter.Sephirot,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();


        if (EnemyAction.PillarofMercy.IsCasting())
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, $"Moving onto Arena center");
            while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(ArenaCenter.Sephirot) > 1)
            {
                Navigator.PlayerMover.MoveTowards(ArenaCenter.Sephirot);
                await Coroutine.Yield();
            }

            await CommonTasks.StopMoving();
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Sephirot
        /// </summary>
        public const uint Sephirot = 4776;

        /// <summary>
        /// Coronal Wind
        /// </summary>
        public const uint CoronalWind = 4780;

    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss: Sephirot.
        /// </summary>
        public static readonly Vector3 Sephirot = new(0f, 0f, 0f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Pillar of Mercy
        /// When this spell casts he targets a party memeber that needs to hide behind a rock, since we can't tell who's targetting just hide
        /// </summary>
        public static readonly HashSet<uint> PillarofMercy = new() {5866,5579};
    }

    private static class AblityTimers
    {
    }
}
