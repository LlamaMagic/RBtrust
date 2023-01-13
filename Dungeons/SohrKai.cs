using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
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
/// Lv. 60.3: Sohr Kai dungeon logic.
/// </summary>
public class SohrKai : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.SohrKai;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.SohrKai;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } =
        new() {EnemyAction.Gallop, EnemyAction.AkhMorn, EnemyAction.FrigidDive};

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2: Bomb
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnstifledPrayer,
            objectSelector: bc =>
                bc.CastingSpellId == EnemyAction.Bomb,
            radiusProducer: bc => 31.0f,
            priority: AvoidancePriority.High));

        // Boss 2: Dark Cloud > Lightning Bolt
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnstifledPrayer,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.LightningBolt,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.Medium));

        // Boss 3: Hallowed Wings Left
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ShatteredRemembrance,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HallowedWingsLeft,
            width: 20f,
            length: 40f,
            xOffset: -10f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 3: Hallowed Wings Right
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ShatteredRemembrance,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HallowedWingsRight,
            width: 20f,
            length: 40f,
            xOffset: 10f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnseveredDespair,
            () => ArenaCenter.ChieftainMoglin,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnstifledPrayer,
            () => ArenaCenter.Poqhiraj,
            outerRadius: 90.0f,
            innerRadius: 19f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ShatteredRemembrance,
            () => ArenaCenter.Hraesvelgr,
            outerRadius: 90.0f,
            innerRadius: 19f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (EnemyAction.HolyBreath.IsCasting())
        {
            await MovementHelpers.Spread(AblityTimers.HolyBreathDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Chieftain Moglin.
        /// </summary>
        public const uint ChieftainMoglin = 4943;

        /// <summary>
        /// Second Boss: Poqhiraj.
        /// </summary>
        public const uint Poqhiraj = 4952;

        /// <summary>
        /// Second Boss: Poqhiraj.
        /// Dark Cloud
        /// </summary>
        public const uint DarkCloud = 4953;

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// </summary>
        public const uint Hraesvelgr = 4954;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Chieftain Moglin.
        /// </summary>
        public static readonly Vector3 ChieftainMoglin = new(-399.5276f, 8f, -157.4418f);

        /// <summary>
        /// Second Boss: Poqhiraj.
        /// </summary>
        public static readonly Vector3 Poqhiraj = new(400f, 25f, 102f);

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// </summary>
        public static readonly Vector3 Hraesvelgr = new(400f, -55f, -400f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Second Boss: Poqhiraj
        /// Bomb
        /// CircleAOE
        /// </summary>
        public const uint Bomb = 6240;

        /// <summary>
        /// Second Boss: Poqhiraj
        /// Gallop
        /// Follow NPCs to get out of the way
        /// </summary>
        public const uint Gallop = 5778;

        /// <summary>
        /// Second Boss: Poqhiraj
        /// Lightning Bolt
        /// AOE cloud lightning
        /// </summary>
        public const uint LightningBolt = 6010;

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// Akh Morn
        /// Stack mechanic
        /// </summary>
        public const uint AkhMorn = 32132;

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// Hallowed Wings left
        /// Line AOE on left side of boss
        /// </summary>
        public const uint HallowedWingsLeft = 32137;

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// Hallowed Wings right
        /// Line AOE on right side of boss
        /// </summary>
        public const uint HallowedWingsRight = 32136;

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// Frigid Dive
        /// Following the NPCs here because RB is stupid
        /// </summary>
        public const uint FrigidDive = 32134;

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// Hallowed Dive
        /// Following the NPCs here because RB is stupid
        /// </summary>
        public const uint HallowedDive = 32131;

        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// Holy Breath
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> HolyBreath = new() {32138};
    }

    private static class AblityTimers
    {
        /// <summary>
        /// Third Boss: Hraesvelgr.
        /// Holy Breath
        /// Spread
        /// </summary>
        public static readonly int HolyBreathDuration = 6_000;
    }
}
