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
/// Lv. 97: Vanguard dungeon logic.
/// </summary>
public class Vanguard : AbstractDungeon
{

    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Vanguard;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Vanguard;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CentralGarage,
            () => ArenaCenter.VanguardCommanderR8,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SafetyInspectionChamber,
            () => ArenaCenter.Protector,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GatekeepsAnvil,
            () => ArenaCenter.ZandertheSnakeskinner,
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
            SubZoneId.CentralGarage => await VanguardCommanderR8(),
            SubZoneId.SafetyInspectionChamber => await Protector(),
            SubZoneId.GatekeepsAnvil => await ZandertheSnakeskinner(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

   /// <summary>
    /// Boss 1: Vanguard Commander R8.
    /// </summary>
    private async Task<bool> VanguardCommanderR8()
    {

        return false;
    }

    /// <summary>
    /// Boss 2: Protector.
    /// </summary>
    private async Task<bool> Protector()
    {

        return false;
    }

    /// <summary>
    /// Boss 3: Zander the Snakeskinner.
    /// </summary>
    private async Task<bool> ZandertheSnakeskinner()
    {

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Vanguard Commander R8.
        /// </summary>
        public const uint VanguardCommanderR8 = 12750;

        /// <summary>
        /// Second Boss: Protector.
        /// </summary>
        public const uint Protector = 12757;

        /// <summary>
        /// Final Boss: Zander the Snakeskinner .
        /// </summary>
        public const uint ZandertheSnakeskinner = 12752;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Magitek Rearguard.
        /// </summary>
        public static readonly Vector3 VanguardCommanderR8 = new(-100f, 7f, 207f);

        /// <summary>
        /// Second Boss: Protector.
        /// </summary>
        public static readonly Vector3 Protector = new(0f, 7f, -100f);

        /// <summary>
        /// Third Boss: Zander the Snakeskinner.
        /// </summary>
        public static readonly Vector3 ZandertheSnakeskinner = new(90f, 12f, -430f);
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
