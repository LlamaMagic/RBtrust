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
public class ContainmentBayZ1T9 : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.ContainmentBayZ1T9;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ContainmentBayZ1T9;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Attach very wide cone avoid pointing out the boss's right, forcing bot to left side
        // Boss spins clockwise and front cleave comes quickly, so disallow less-safe right side
        // Position + rotation will auto-update as the boss moves + turns!
        /*
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayZ1T9,
            objectSelector: (obj) => obj.NpcId == EnemyNpc.Zurvan && obj.IsTargetable,
            leashPointProducer: () => ArenaCenter.Zurvan,
            leashRadius: 100f,
            rotationDegrees: 0f,
            radius: -180f,
            arcDegrees: 280f,
            priority: AvoidancePriority.Medium);
            */

        // Safe Area
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayZ1T9,
            () => ArenaCenter.SafeArea,
            outerRadius: 90.0f,
            innerRadius: 8.5f,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayZ1T9,
            () => ArenaCenter.Zurvan,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Sophia
        /// </summary>
        public const uint Zurvan = 5567;

    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss: Sephirot.
        /// </summary>
        public static readonly Vector3 Zurvan = new(0f, 0f, 0f);

        /// <summary>
        /// Boss: Sephirot.
        /// </summary>
        public static readonly Vector3 SafeArea = new(-0.143432f, 1.31130f, 12.7033f);
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
