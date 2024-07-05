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

        /*
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheThirdArmory,
            () => ArenaCenter.MagitekRearguard,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TrainingGrounds,
            () => ArenaCenter.MagitekHexadron,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.HalloftheScarletSwallow,
            () => ArenaCenter.HypertunedGrynewaht,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);
            */
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
        /// First Boss: Magitek Rearguard.
        /// </summary>
        public const uint MagitekRearguard = 6200;

        /// <summary>
        /// Second Boss: Magitek Hexadron.
        /// </summary>
        public const uint MagitekHexadron = 6203;

        /// <summary>
        /// Second Boss: Hexadron Bit.
        /// </summary>
        public const uint HexadroneBit = 6204;

        /// <summary>
        /// Final Boss: Hypertuned Grynewaht .
        /// </summary>
        public const uint HypertunedGrynewaht = 6205;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Magitek Rearguard.
        /// </summary>
        public static readonly Vector3 MagitekRearguard = new(125f, 40.5f, 17.5f);

        /// <summary>
        /// Second Boss: Magitek Hexadron.
        /// </summary>
        public static readonly Vector3 MagitekHexadron = new(-240f, 45.5f, 130.5f);

        /// <summary>
        /// Third Boss: Hypertuned Grynewaht.
        /// </summary>
        public static readonly Vector3 HypertunedGrynewaht = new(-240f, 67f, -197f);

        /// <summary>
        /// Third Boss: Bomb Drop Spot.
        /// </summary>
        public static readonly Vector3 BombDropSpot = new(-257.6511f, 67f, -179.8466f);

        /// <summary>
        /// Third Boss: Bomb Safe Spot.
        /// </summary>
        public static readonly Vector3 BombSafeSpot = new(-224.3772f, 67f, -214.2821f);
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
