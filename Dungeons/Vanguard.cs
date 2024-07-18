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
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.HeavyBlastCannon, EnemyAction.HomingCannon };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Rush
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CentralGarage,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Rush,
            width: 6f,
            length: 40f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 1: Enhanced Mobility
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CentralGarage,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.EnhancedMobilityCircle or EnemyAction.EnhancedMobilityCircle2,
            radiusProducer: bc => 17.0f,
            priority: AvoidancePriority.Medium));

        // Boss 1: Enhancd Mobility Donut
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CentralGarage,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.EnhancedMobilityDonut or EnemyAction.EnhancedMobilityDonut2,
            outerRadius: 40.0f,
            innerRadius: 10.0f,
            priority: AvoidancePriority.High);

        // Boss 1: Electrosurge
        // Boss 2: Tracking Bolt
        // Boss 3: Soulbane Shock
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.CentralGarage or (uint)SubZoneId.SafetyInspectionChamber or (uint)SubZoneId.VanguardControlRoom,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.Electrosurge or EnemyAction.TrackingBolt or EnemyAction.SoulbaneShock && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        /*
        // Boss 2: Homing cannon
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SafetyInspectionChamber,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HomingCannon,
            width: 2f,
            length: 40f,
            yOffset: 0f,
            priority: AvoidancePriority.High);
            */

        // Boss 2: Blast Cannon
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SafetyInspectionChamber,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.BlastCannon,
            width: 6f,
            length: 40f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CentralGarage,
            innerWidth: 33.0f,
            innerHeight: 33.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.VanguardCommanderR8 },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SafetyInspectionChamber,
            innerWidth: 22.0f,
            innerHeight: 38.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Protector },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.VanguardControlRoom,
            () => ArenaCenter.ZandertheSnakeskinner,
            outerRadius: 90.0f,
            innerRadius: 16.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        if (WorldManager.SubZoneId is (uint)SubZoneId.SafetyInspectionChamber)
        {
            SidestepPlugin.Enabled = false;
        }
        else
        {
            SidestepPlugin.Enabled = true;
        }

        bool result = currentSubZoneId switch
        {
            SubZoneId.CentralGarage => await VanguardCommanderR8(),
            SubZoneId.SafetyInspectionChamber => await Protector(),
            SubZoneId.VanguardControlRoom => await ZandertheSnakeskinner(),
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
        if (EnemyAction.AerialOffensive.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            await MovementHelpers.GetClosestAlly.Follow();
        }
        else
        {
            SidestepPlugin.Enabled = true;
        }

        return false;
    }

    /// <summary>
    /// Boss 2: Protector.
    /// </summary>
    private async Task<bool> Protector()
    {
        if (EnemyAction.Bombardment.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

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
        /// Second Boss: Fulminous Fence.
        /// </summary>
        public const uint FulminousFence = 13563;

        /// <summary>
        /// Final Boss: Zander the Snakeskinner .
        /// </summary>
        public const uint ZandertheSnakeskinner = 12752;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Vanguard Commander R8.
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
        /// Vanguard Commander R8
        /// Rush
        /// line oE
        /// </summary>
        public const uint Rush = 36569;

        public static readonly HashSet<uint> RushCasting = new() { 36569 };

        /// <summary>
        /// Vanguard Commander R8
        /// Enhanced Mobility
        /// line oE
        /// </summary>
        public const uint EnhancedMobilityCircle = 36563;

        public const uint EnhancedMobilityCircle2 = 36564;

        /// <summary>
        /// Vanguard Commander R8
        /// Enhanced Mobility
        /// DonutA AoE
        /// </summary>
        public const uint EnhancedMobilityDonut = 37191;

        public const uint EnhancedMobilityDonut2 = 36560;

        /// <summary>
        /// HVanguard Commander R8
        /// Aerial Offensive
        /// Follow
        /// </summary>
        public static readonly HashSet<uint> AerialOffensive = new() { 36570 };


        /// <summary>
        /// Vanguard Commander R8
        /// Electrosurge
        /// spread
        /// </summary>
        public const uint Electrosurge = 36573;

        /// <summary>
        /// Protector
        /// Tracking Bolt
        /// spread
        /// </summary>
        public const uint TrackingBolt = 37349;

        /// <summary>
        /// Protector
        /// Homing Cannon
        /// Straight line AOE
        /// </summary>
        public const uint HomingCannon = 37155;

        /// <summary>
        /// Protector
        /// Shock
        /// Small Circle AoE that drops the same time as HomingCannon
        /// </summary>
        public const uint Shock = 37156;

        /// <summary>
        /// Protector
        /// Bombardment
        /// Boss turns around in circles and sprays electricity while bombs drop. Follow to dodge
        /// </summary>
        public static readonly HashSet<uint> Bombardment = new() { 39016 };


        /// <summary>
        /// Protector
        /// Electrowhirl
        /// Boss turns around in circles and sprays electricity while bombs drop
        /// </summary>
        public static readonly HashSet<uint> Electrowhirl = new() { 37160 };

        /// <summary>
        /// Protector
        /// Blast Cannon
        /// Follow to dodge
        /// </summary>
        public const uint BlastCannon = 37151;

        /// <summary>
        /// Protector
        /// Heavy Blast Cannon
        /// Stack
        /// </summary>
        public const uint HeavyBlastCannon = 37345;

        /// <summary>
        /// Zander the Snakeskinner
        /// Soulbane Shock
        /// Stack
        /// </summary>
        public const uint SoulbaneShock = 37922;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
