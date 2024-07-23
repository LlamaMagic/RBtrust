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
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.CollectiveAgony, EnemyAction.ExtrasensoryField, EnemyAction.Psychokinesis };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Poison boils that form on the ground, want to avoid them
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ResourceTransportElevator,
            objectSelector: eo => eo.NpcId == EnemyNpc.PoisonBoil,
            radiusProducer: eo => 4.0f,
            priority: AvoidancePriority.High));

        // Boss 2: Synchroshot
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SurveillanceRoom,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.LaserLash,
            width: 10f,
            length: 40f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 2: Laser Lash
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SurveillanceRoom,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Synchroshot,
            width: 5f,
            length: 40f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss 2: Bionic Thrash
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SurveillanceRoom,
            objectSelector: (bc) => bc.CastingSpellId is EnemyAction.BionicThrash or EnemyAction.BionicThrash2,
            leashPointProducer: () => ArenaCenter.Deceiver,
            leashRadius: 40.0f,
            rotationDegrees: 0f,
            radius: 40.0f,
            arcDegrees: 90f);

        // Hard Stomp
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)5017,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.HardStomp,
            radiusProducer: bc => 17.0f,
            priority: AvoidancePriority.Medium));

        // Boss 1: Poison Heart
        // Boss 2: Electray
        // Boss 3: Whorl of the Mind
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.ResourceTransportElevator or (uint)SubZoneId.SurveillanceRoom or (uint)SubZoneId.EnhancementTestingGrounds && !EnemyAction.Surge.IsCasting(),
            objectSelector: bc => bc.CastingSpellId is EnemyAction.Electray or EnemyAction.PoisonHeart or EnemyAction.WhorloftheMind && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ResourceTransportElevator,
            innerWidth: 30.0f,
            innerHeight: 30.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Herpekaris },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SurveillanceRoom,
            innerWidth: 31.0f,
            innerHeight: 39.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Deceiver },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.EnhancementTestingGrounds,
            innerWidth: 29.0f,
            innerHeight: 37.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.AmbrosetheUndeparted },
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        if (WorldManager.SubZoneId is (uint)SubZoneId.SurveillanceRoom)
        {
            SidestepPlugin.Enabled = false;
        }
        else
        {
            SidestepPlugin.Enabled = true;
        }

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
        if (EnemyAction.Surge.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

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
        /// First Boss: Herpekaris.
        /// </summary>
        public const uint PoisonBoil = 2006588;

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
        /// Third Boss: Ambrose the Undeparted.
        /// </summary>
        public static readonly Vector3 AmbrosetheUndeparted = new(190f, 0f, 0f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Herpekaris
        /// Collective Agony
        /// Stack
        /// </summary>
        public const uint CollectiveAgony = 8357;

        /// <summary>
        /// Herpekaris
        /// Poison Heart
        /// Spread
        /// </summary>
        public const uint PoisonHeart = 37921;

        /// <summary>
        /// Deceiver
        /// Surge
        /// Follow NPC
        /// </summary>
        public static readonly HashSet<uint> Surge = new() { 39736 };

        /// <summary>
        /// Deceiver
        /// Electray
        /// Spread
        /// </summary>
        public const uint Electray = 38320;

        /// <summary>
        /// Deceiver
        /// Bionic Thrash
        /// Cone AoE
        /// </summary>
        public const uint BionicThrash = 36369;

        /// <summary>
        /// Deceiver
        /// Bionic Thrash
        /// Cone AoE
        /// </summary>
        public const uint BionicThrash2 = 36370;

        /// <summary>
        /// Deceiver
        /// Synchroshot
        /// Line AoE
        /// </summary>
        public const uint Synchroshot = 36372;

        /// <summary>
        /// Deceiver
        /// Laser Lash
        /// Lasers from the left and right come out, two are broken. Maybe 38807
        /// </summary>
        public const uint LaserLash = 36366;

        /// <summary>
        /// Origenics Automatoise
        /// Hard Stomp
        /// Large AOE that SideStep doesn't pick up
        /// </summary>
        public const uint HardStomp = 39149;

        /// <summary>
        /// Ambrose the Undeparted
        /// Whorl of the Mind
        /// Spread
        /// </summary>
        public const uint WhorloftheMind = 36438;

        /// <summary>
        /// Ambrose the Undeparted
        /// Extrasensory Field
        /// Follow
        /// </summary>
        public const uint ExtrasensoryField = 36432;

        /// <summary>
        /// Ambrose the Undeparted
        /// Psychokinesis
        /// Orders a spear to swing across the battlefield, easiest to just follow dodge
        /// </summary>
        public const uint Psychokinesis = 38929;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
