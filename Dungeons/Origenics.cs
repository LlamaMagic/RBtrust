using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 99: Origenics dungeon logic.
/// </summary>
public class Origenics : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Origenics;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Origenics;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() {  };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ResourceTransportElevator,
            () => ArenaCenter.Herpekaris,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SurveillanceRoom,
            () => ArenaCenter.Deceiver,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.EnhancementTestingGrounds,
            () => ArenaCenter.AmbrosetheUndeparted,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        bool result = currentSubZoneId switch
        {
            SubZoneId.ResourceTransportElevator => await Herpekaris(),
            SubZoneId.SurveillanceRoom => await Deceiver(),
            SubZoneId.EnhancementTestingGrounds => await AmbrosetheUndeparted(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Herpekaris.
    /// </summary>
    private async Task<bool> Herpekaris()
    {

        return false;
    }

    /// <summary>
    /// Boss 2: Deceiver.
    /// </summary>
    private async Task<bool> Deceiver()
    {

        return false;
    }

    /// <summary>
    /// Boss 3: Ambrose the Undeparted.
    /// </summary>
    private async Task<bool> AmbrosetheUndeparted()
    {

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Herpekaris.
        /// </summary>
        public const uint Herpekaris = 12741;

        /// <summary>
        /// Second Boss: Deceiver.
        /// </summary>
        public const uint Deceiver = 12693;

        /// <summary>
        /// Final Boss: Ambrose the Undeparted .
        /// </summary>
        public const uint AmbrosetheUndeparted = 12695;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Herpekaris.
        /// </summary>
        public static readonly Vector3 Herpekaris = new(-88f, -120f, -180f);

        /// <summary>
        /// Second Boss: Deceiver.
        /// </summary>
        public static readonly Vector3 Deceiver = new(-172f, -94f, -142f);

        /// <summary>
        /// Third Boss: Bomb Safe Spot.
        /// </summary>
        public static readonly Vector3 AmbrosetheUndeparted = new(190f, 0f, 0f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Magitek Hexadron
        /// Magitek Missiles
        /// Stack
        /// </summary>
        public const uint MagitekMissiles = 8357;

        /// <summary>
        /// Hypertuned Grynewaht
        /// Thermobaric Charge
        /// Run away
        /// </summary>
        public static readonly HashSet<uint> ThermobaricCharge = new() { 8357 };

        /// <summary>
        /// Hypertuned Grynewaht
        /// Clean Cut
        /// Straight line avoid
        /// </summary>
        public const uint CleanCut = 8369;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
