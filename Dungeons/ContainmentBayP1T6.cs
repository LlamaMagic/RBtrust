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
public class ContainmentBayP1T6 : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.ContainmentBayP1T6;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ContainmentBayP1T6;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Thunder III
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayP1T6,
            objectSelector: c => c.CastingSpellId == EnemyAction.ThunderIII,
            outerRadius: 40.0f,
            innerRadius: 6.0F,
            priority: AvoidancePriority.Medium);

        // Stay out of the ice
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<GameObject>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayP1T6,
            objectSelector: bc => bc.NpcId == EnemyNpc.IcePuddle && bc.IsVisible,
            radiusProducer: bc => 6f,
            priority: AvoidancePriority.High));

        // Line AOE
        // Cone AOEs
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Onrush,
            leashPointProducer: () => Core.Player.Location,
            leashRadius: 100f,
            rotationDegrees: 0f,
            radius: 80f,
            arcDegrees: 50f,
            priority: AvoidancePriority.Medium);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayP1T6,
            () => ArenaCenter.Sophia,
            outerRadius: 90.0f,
            innerRadius: 14.9f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // So turns out this witch casts both copies of Quasar one after the other
        // So can't just rely on the spell ID to know which side to run to
        // Leaving this here as it's got a 50/50 shot at being right
        if (EnemyAction.QuasarEast.IsCasting())
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, $"Moving to the west to dodge Quasar");
            while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(ArenaCenter.WestSide) > 1)
            {
                Navigator.PlayerMover.MoveTowards(ArenaCenter.WestSide);
                await Coroutine.Yield();
            }

            await CommonTasks.StopMoving();
            await Coroutine.Sleep(7000);
        }

        if (EnemyAction.QuasarWest.IsCasting())
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, $"Moving to the east to dodge Quasar");
            while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(ArenaCenter.EastSide) > 1)
            {
                Navigator.PlayerMover.MoveTowards(ArenaCenter.EastSide);
                await Coroutine.Yield();
            }

            await CommonTasks.StopMoving();
            await Coroutine.Sleep(7000);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Sophia
        /// </summary>
        public const uint Sophia = 4776;

        /// <summary>
        /// IcePuddle
        /// </summary>
        public const uint IcePuddle = 2007449;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss: Sophia.
        /// </summary>
        public static readonly Vector3 Sophia = new(0f, 0f, 0f);

        /// <summary>
        /// Boss: Sophia.
        /// </summary>
        public static readonly Vector3 WestSide = new(-13.72405f, 0f, -0.0706864f);

        /// <summary>
        /// Boss: Sophia.
        /// </summary>
        public static readonly Vector3 EastSide = new(13.52379f, 0f, 0.09186649f);
    }



    private static class EnemyAction
    {
        /// <summary>
        /// Thunder III
        /// AoE Donut around boss
        /// </summary>
        public const uint ThunderIII = 6514;

        /// <summary>
        /// Onrush
        /// Line AOE
        /// </summary>
        public const uint Onrush = 6533;

        /// <summary>
        /// Quasar
        /// Pulls to the west, so go east
        /// </summary>
        public static readonly HashSet<uint> QuasarWest = new() {6510};

        /// <summary>
        /// Quasar
        /// Pulls to the east, so go west
        /// </summary>
        public static readonly HashSet<uint> QuasarEast = new() {6511};

    }

    private static class AblityTimers
    {
    }
}
