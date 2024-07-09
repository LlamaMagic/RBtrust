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
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Superbolt, EnemyAction.Compression, EnemyAction.Overexposure, EnemyAction.LightofDevotion };

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
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.ImmuneResponseFront,
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

        // Boss 2: Static Spark
        // Boss 3: Electray
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.CorruptedMemoryCache or (uint)SubZoneId.Reascension,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.StaticSpark or EnemyAction.Electray && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss 3: Unknown Lazer
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Unknown,
            width: 6f,
            length: 120f,
            yOffset: -60f,
            priority: AvoidancePriority.High);

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
            innerHeight: 29.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Amalgam },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Reascension,
            innerWidth: 39.0f,
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
                await MovementHelpers.Spread(QuarantineDuration, 7f);
            }
            else
            {
                await MovementHelpers.GetClosestAlly.Follow(3f);
            }
        }

        // Follow the NPCs while the intererons are present.
        // Sidestep detects the omens, but the spell cast is so fast that the character can't move into position quick enough if you rely only on spell cast.
        // The NPCs are good at dodging
        if (Core.Me.InCombat && InterfornPresent)
        {
            await MovementHelpers.GetClosestAlly.Follow();
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
        public static readonly Vector3 Eliminator = new(-760f, -474f, -648f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Antivirus X
        /// Quarantine
        /// Stack
        /// </summary>
        public static readonly HashSet<uint> Quarantine = new() { 36384 };

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
        /// Superbolt
        /// Stack
        /// </summary>
        public const uint Superbolt = 36333;

        /// <summary>
        /// Eliminator
        /// Electray
        /// Spread
        /// </summary>
        public const uint Electray = 36333;

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
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
