using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 28: Haukke Manor dungeon logic.
/// </summary>
public class HaukkeManor : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.HaukkeManor;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.HaukkeManor;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 3: PetrifyingEye aoe-gaze attack
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WhiteHall,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.PetrifyingEye && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.High));

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
        /// First Boss: Manor Claviger.
        /// </summary>
        public const uint ManorClaviger = 423;

        /// <summary>
        /// Second Boss: Manor Steward.
        /// </summary>
        public const uint ManorSteward = 427;

        /// <summary>
        /// Second Boss: Manor Jester.
        /// </summary>
        public const uint ManorJester = 426;

        /// <summary>
        /// Final Boss Add: Lady Amandine.
        /// </summary>
        public const uint LadyAmandine = 422;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Manor Claviger.
        /// </summary>
        public static readonly Vector3 ManorClaviger = new(1.5f, 0f, 0f);

        /// <summary>
        /// Second Boss: Manor Jester & Manor Steward.
        /// </summary>
        public static readonly Vector3 JesterandSteward = new(0f, -19f, 0f);

        /// <summary>
        /// Third Boss: Octomammoth.
        /// </summary>
        public static readonly Vector3 Octomammoth = new(0f, 17f, 0f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Lady Amandine"/>'s Petrifying Eye.
        ///
        /// Need to turn away to avoid it
        /// </summary>
        public const uint PetrifyingEye = 28648;
    }
}
