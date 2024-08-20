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
/// Lv. 100: Alexandria dungeon logic.
/// </summary>
public class Alexandria : AbstractDungeon
{
    private const int QuarantineDuration = 10_000;

    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Alexandria;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Alexandria;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Impact, EnemyAction.Superbolt, EnemyAction.Compression, EnemyAction.Overexposure, EnemyAction.LightofDevotion };

    private static GameObject interferonC => GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.InterferonC)
        .FirstOrDefault(bc => bc.IsVisible); // +

    private static GameObject interferonR => GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.InterferonR)
        .FirstOrDefault(bc => bc.IsVisible); // O

    private static bool InterfornPresent => interferonC != null || interferonR != null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Immune Response Front
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.VolatileMemory && !InterfornPresent,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.ImmuneResponseFront && !InterfornPresent,
            leashPointProducer: () => ArenaCenter.AntivirusX,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 125f);

        // Boss 1: Immune Response Sides
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.VolatileMemory && !InterfornPresent,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.ImmuneResponseSides,
            leashPointProducer: () => ArenaCenter.AntivirusX,
            leashRadius: 40.0f,
            rotationDegrees: -180.0f,
            radius: 40.0f,
            arcDegrees: 255f);

        // Boss 1: Quantine
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && !Core.Player.IsTank() && WorldManager.SubZoneId is (uint)SubZoneId.VolatileMemory,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.QuarantineConst && bc.SpellCastInfo.TargetId != Core.Player.ObjectId && Core.Player.Distance2D(MovementHelpers.GetClosestTank.Location) < 9f,
            radiusProducer: bc => 20f,
            locationProducer: bc => MovementHelpers.GetClosestTank.Location);

        // Boss 2: Static Spark
        // Boss 2: Voltburst
        // Boss 3: Electray
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.CorruptedMemoryCache or (uint)SubZoneId.Reascension,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.Voltburst or EnemyAction.StaticSpark or EnemyAction.Electray && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss 2: Supercell Matrix
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.SupercellMatrix,
            width: 60f,
            length: 28.5f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 2: Supercell Matrix Lines
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.SupercellMatrixLine,
            width: 8.5f,
            length: 120f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 2: Split Current
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.SplitCurrent,
            width: 20.5f,
            length: 120f,
            yOffset: -20f,
            xOffset: 15f,
            priority: AvoidancePriority.High);
        // Boss 2: Split Current
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.SplitCurrent,
            width: 20.5f,
            length: 120f,
            yOffset: -20f,
            xOffset: -15f,
            priority: AvoidancePriority.High);

        // Boss 2: Centralized Current
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.CentralizedCurrent,
            width: 16f,
            length: 120f,
            yOffset: -40f,
            priority: AvoidancePriority.High);

        // Boss 2: Ternary Charge Inner
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.TernaryChargeInner,
            radiusProducer: eo => 11.0f,
            priority: AvoidancePriority.High));

        // Boss 2: Ternary Charge Outer
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache && !EnemyAction.TernaryChargeHash.IsCasting(),
            objectSelector: c => c.CastingSpellId == EnemyAction.TernaryChargeOuter,
            outerRadius: 40.0f,
            innerRadius: 9.0F,
            priority: AvoidancePriority.Medium);

        // Boss 3: Unknown Lazer
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.Unknown,
            leashPointProducer: () => ArenaCenter.Eliminator,
            leashRadius: 40.0f,
            rotationDegrees: 0f,
            radius: 40.0f,
            arcDegrees: 30f);

        // Boss 3: Terminate
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Terminate,
            width: 12f,
            length: 40f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 3: Halo of Destruction
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            objectSelector: c => c.CastingSpellId == EnemyAction.HaloofDestruction,
            outerRadius: 40.0f,
            innerRadius: 4.0F,
            priority: AvoidancePriority.Medium);

        // Boss 3: Partition Left
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            objectSelector: (bc) => bc.CastingSpellId is EnemyAction.PartitionLeft or EnemyAction.PartitionLeft2,
            leashPointProducer: () => ArenaCenter.AntivirusX,
            leashRadius: 40.0f,
            rotationDegrees: -90f,
            radius: 40.0f,
            arcDegrees: 180f);

        // Boss 3: Partition Right
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            objectSelector: (bc) => bc.CastingSpellId is EnemyAction.PartitionRight,
            leashPointProducer: () => ArenaCenter.AntivirusX,
            leashRadius: 40.0f,
            rotationDegrees: 90f,
            radius: 40.0f,
            arcDegrees: 180f);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.VolatileMemory,
            innerWidth: 39.0f,
            innerHeight: 29.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.AntivirusX },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CorruptedMemoryCache,
            innerWidth: 39.0f,
            innerHeight: 39.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Amalgam },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            innerWidth: 29.0f,
            innerHeight: 29.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Eliminator },
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        if (WorldManager.SubZoneId is (uint)SubZoneId.VolatileMemory or (uint)SubZoneId.CorruptedMemoryCache or (uint)SubZoneId.Reascension)
        {
            SidestepPlugin.Enabled = false;
        }
        else
        {
            SidestepPlugin.Enabled = true;
        }

        bool result = currentSubZoneId switch
        {
            SubZoneId.VolatileMemory => await AntivirusX(),
            SubZoneId.CorruptedMemoryCache => await Amalgam(),
            SubZoneId.Reascension => await Eliminator(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Antivirus X.
    /// </summary>
    private async Task<bool> AntivirusX()
    {
        if (EnemyAction.Quarantine.IsCasting())
        {
            // If you're on tank you want to spread during Quarantine as it does an AOE tank buster on the tank
            // Otherwise you want to stack.
            if (Core.Player.IsTank())
            {
                await MovementHelpers.Spread(QuarantineDuration, 10f);
            }

            if (!Core.Player.IsTank() && Core.Player.Distance2D(MovementHelpers.GetClosestTank.Location) > 7.5f)
            {
                await MovementHelpers.GetClosestDps.Follow();
            }
        }

        // Follow the NPCs while the intererons are present.
        // Sidestep detects the omens, but the spell cast is so fast that the character can't move into position quick enough if you rely only on spell cast.
        // The NPCs are good at dodging
        if (Core.Me.InCombat && InterfornPresent)
        {
            await MovementHelpers.GetClosestDps.Follow();
        }

        return false;
    }

    /// <summary>
    /// Boss 2: Amalgam.
    /// </summary>
    private async Task<bool> Amalgam()
    {
        return false;
    }

    /// <summary>
    /// Boss 3: Eliminator.
    /// </summary>
    private async Task<bool> Eliminator()
    {
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Antivirus X.
        /// </summary>
        public const uint AntivirusX = 12844;

        /// <summary>
        /// First Boss: Interferon R
        /// </summary>
        public const uint InterferonR = 12843;

        /// <summary>
        /// First Boss: Interferon C
        /// </summary>
        public const uint InterferonC = 12842;

        /// <summary>
        /// Second Boss: Amalgam.
        /// </summary>
        public const uint Amalgam = 12864;

        /// <summary>
        /// Final Boss: Eliminator .
        /// </summary>
        public const uint Eliminator = 12729;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: <see cref="EnemyNpc.AntivirusX"/>.
        /// </summary>
        public static readonly Vector3 AntivirusX = new(852f, 46f, 823f);

        /// <summary>
        /// Second Boss: Amalgam.
        /// </summary>
        public static readonly Vector3 Amalgam = new(-533f, -466f, -373f);

        /// <summary>
        /// Third Boss: Eliminator.
        /// </summary>
        public static readonly Vector3 Eliminator = new(-759f, -474f, -648f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Antivirus X
        /// Quarantine
        /// Stack
        /// </summary>
        public static readonly HashSet<uint> Quarantine = new() { 36384 };

        public const uint QuarantineConst = 36384;

        /// <summary>
        /// Antivirus X
        /// Immune Response
        /// Frontal cone
        /// </summary>
        public const uint ImmuneResponseFront = 36378;

        /// <summary>
        /// Antivirus X
        /// Immune Response
        ///  Cone damage on both left and right side
        /// </summary>
        public const uint ImmuneResponseSides = 36380;

        /// <summary>
        /// Amalgam
        /// Static Spark
        /// Spread
        /// </summary>
        public const uint StaticSpark = 36334;

        /// <summary>
        /// Amalgam
        /// Supercell Matrix
        /// Cone type AoE starting from bottom right of arena
        /// </summary>
        public const uint SupercellMatrix = 39136;

        /// <summary>
        /// Amalgam
        /// Supercell Matrix
        /// Straight line AoEs
        /// </summary>
        public const uint SupercellMatrixLine = 39138;

        /// <summary>
        /// Amalgam
        /// Split Current
        /// Line AoE, could also be 36329, or 36330, or 36331
        /// </summary>
        public const uint SplitCurrent = 36331;

        /// <summary>
        /// Amalgam
        /// Centralized Current
        /// Line AoE right through the middle of the arena
        /// </summary>
        public const uint CentralizedCurrent = 36327;

        /// <summary>
        /// Amalgam
        /// Superbolt
        /// Stack
        /// </summary>
        public const uint Superbolt = 36333;

        /// <summary>
        /// Amalgam
        /// Voltburst
        /// Small AoEs to dodge
        /// </summary>
        public const uint Voltburst = 36336;

        /// <summary>
        /// Amalgam
        /// Ternary Charge
        /// Three wave attack
        /// </summary>
        public const uint TernaryChargeInner = 39253;

        public static readonly HashSet<uint> TernaryChargeHash = new() { 39253 };

        /// <summary>
        /// Amalgam
        /// Ternary Charge
        /// Three wave attack
        /// </summary>
        public const uint TernaryChargeOuter = 39256;

        /// <summary>
        /// Eliminator
        /// Unkown
        /// Small line AOE
        /// </summary>
        public const uint Unknown = 36783;

        /// <summary>
        /// Eliminator
        /// Compression
        /// We want to move to the edge of the blue circle with the other NPCs
        /// </summary>
        public const uint Compression = 36792;

        /// <summary>
        /// Eliminator
        /// Electray
        /// Spread
        /// </summary>
        public const uint Electray = 39243;

        /// <summary>
        /// Eliminator
        /// Overexposure
        /// Stack
        /// </summary>
        public const uint Overexposure = 36779;

        /// <summary>
        /// Eliminator
        /// Light of Devotion
        /// Stack
        /// </summary>
        public const uint LightofDevotion = 36785;

        /// <summary>
        /// Eliminator
        /// Halo of Destruction
        /// Donut AoE
        /// </summary>
        public const uint HaloofDestruction = 36776;

        /// <summary>
        /// Eliminator
        /// Partition
        /// Left Half of room
        /// </summary>
        public const uint PartitionLeft = 39007;

        /// <summary>
        /// Eliminator
        /// Partition
        /// Right Half Room Wide Cone AoE
        /// </summary>
        public const uint PartitionRight = 39249;

        /// <summary>
        /// Eliminator
        /// Partition
        /// Right Half Room Wide Cone AoE
        /// </summary>
        public const uint PartitionLeft2 = 39238;

        /// <summary>
        /// Eliminator
        /// Terminate
        /// Line AoE to dodge
        /// </summary>
        public const uint Terminate = 36773;

        /// <summary>
        /// Eliminator
        /// Impact
        /// Stand on the edge of impact to get pushed back
        /// </summary>
        public const uint Impact = 36794;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
