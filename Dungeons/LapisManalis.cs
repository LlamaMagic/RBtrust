using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90.3: Lapis Manalis dungeon logic.
/// </summary>
public class LapisManalis : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.LapisManalis;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.LapisManalis;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        EnemyAction.RoarOfAlbion,
        EnemyAction.Hydrofall,
        EnemyAction.HydraulicRam,
    };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Icy Throes
        AvoidanceManager.AddAvoidLocation(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius,
            locationProducer: bc => bc.SpellCastInfo?.CastLocation ?? bc.TargetGameObject.Location,
            collectionProducer: () => GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(bc => bc.CastingSpellId is EnemyAction.IcyThroesGround or EnemyAction.IcyThroesPlayers));

        // Boss 1: Wild Beasts Stampede
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            objectSelector: bc => bc.NpcId is EnemyNpc.WildBeastsDummy,
            width: 5f,
            length: 50f,
            yOffset: -5f,
            priority: AvoidancePriority.Medium);

        // Boss 1: Left Slam
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.LeftSlam,
            width: 24f,
            length: 80f,
            xOffset: -12f,
            yOffset: -40f,
            priority: AvoidancePriority.Medium);

        // Boss 1: Right Slam
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.RightSlam,
            width: 24f,
            length: 80f,
            xOffset: 12f,
            yOffset: -40f,
            priority: AvoidancePriority.Medium);

        // Boss 2: Waning Cycle
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.WaningCycleDonut,
            outerRadius: 90.0f,
            innerRadius: 10.0f,
            priority: AvoidancePriority.Medium);

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.WaningCycleDonut,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.Medium));

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.WaningCycleCircle && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 1500,
            radiusProducer: bc => 11.0f,
            priority: AvoidancePriority.Medium));

        // Boss 2: Waxing Cycle
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.WaxingCycleCircle,
            radiusProducer: bc => 10.0f,
            priority: AvoidancePriority.Medium));

        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.WaxingCycleCircle,
            outerRadius: 90.0f,
            innerRadius: 12.0f,
            priority: AvoidancePriority.Medium);

        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.WaxingCycleDonut && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 1500,
            outerRadius: 90.0f,
            innerRadius: 9.0f,
            priority: AvoidancePriority.Medium);

        // Boss 2: Soul Scythe
        AvoidanceManager.AddAvoidLocation(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius,
            locationProducer: bc => bc.SpellCastInfo?.CastLocation ?? bc.TargetGameObject.Location,
            collectionProducer: () => GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(bc => bc.CastingSpellId is EnemyAction.SoulScythe));

        // Boss 2: Scarecrow Chase Dummy Cast Pre-position
        // TODO: Detect head markers 1-4 to determine order sooner and pre-position independently from party
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum
                && GameObjectManager.GetObjectsOfType<BattleCharacter>()
                    .Any(bc => bc.CastingSpellId == EnemyAction.ScarecrowChaseDummy),
            objectSelector: bc => PartyManager.VisibleMembers.Any(pm => pm.BattleCharacter.ObjectId == bc.ObjectId),
            outerRadius: 90.0f,
            innerRadius: 8.0f);

        // Boss 2: Scarecrow Chase
        AvoidanceHelpers.AddAvoidCross<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.ScarecrowChase,
            thickness: 10.0f,
            length: 60.0f,
            rotationProducer: bc => (float)(1.0 / 4.0 * Math.PI));

        // Boss 2: Tenebrism > Towers
        AvoidanceHelpers.AddAvoidDonut(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum
                && Core.Player.GetAuraById(PartyAura.GlassyEyed)?.TimeLeft > 2.0f,
            collectionProducer: () => new[]
            {
                MechanicLocation.TenebrismTowerNorth,
                MechanicLocation.TenebrismTowerSouth,
                MechanicLocation.TenebrismTowerEast,
                MechanicLocation.TenebrismTowerWest,
            },
            outerRadius: 10.0f,
            innerRadius: 5.0f,
            priority: AvoidancePriority.High);

        // Take up space outside of towers so we end up in a tower
        AvoidanceHelpers.AddAvoidDonut(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum
                && Core.Player.GetAuraById(PartyAura.GlassyEyed)?.TimeLeft > 2.0f,
            locationProducer: () => ArenaCenter.GalateaMagna,
            outerRadius: 90.0f,
            innerRadius: 12.0f,
            priority: AvoidancePriority.High);

        // Block towers occupied by other players
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            objectSelector: bc => bc.HasAura(PartyAura.GlassyEyed),
            radiusProducer: bc => 8.0f);

        // Boss 2: Tenebrism > Glassy-Eyed gaze debuff
        AvoidanceManager.AddAvoidLocation(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum
                && Core.Player.GetAuraById(PartyAura.GlassyEyed)?.TimeLeft <= 2.0f,
            radius: 17.0f,
            locationProducer: () => ArenaCenter.GalateaMagna);

        // Boss 3: Antediluvian
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Antediluvian && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds > 3500,
            radiusProducer: bc => 9.5f);

        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Antediluvian && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 3500,
            radiusProducer: bc => 15.0f);

        // Boss 3: Body Slam
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.BodySlamCircle,
            radiusProducer: bc => 9.0f);

        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.BodySlamCircle && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 2000,
            outerRadius: 90.0f,
            innerRadius: 11.0f,
            priority: AvoidancePriority.Medium);

        // Boss 3: Neap Tide
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.GetAuraById(PartyAura.NeapTide)?.TimeLeft < 6.0f,
            radiusProducer: bc => 6.0f,
            priority: AvoidancePriority.Medium));

        // Boss 3: Spring Tide
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.GetAuraById(PartyAura.SpringTide)?.TimeLeft < 6.0f,
            outerRadius: 90.0f,
            innerRadius: 3.0f,
            priority: AvoidancePriority.Medium);

        // Boss 3: Hydrovent
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Hydrovent,
            radius: 6.0f,
            locationProducer: bc => bc.SpellCastInfo.CastLocation);

        // Boss 3: Void Miasma
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.VoidMiasma,
            leashPointProducer: () => ArenaCenter.Cagnazzo,
            leashRadius: 60.0f,
            rotationDegrees: 0f,
            radius: 60.0f,
            arcDegrees: 24f);

        // Boss 3: Lifescleaver
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Lifescleaver,
            leashPointProducer: () => ArenaCenter.Cagnazzo,
            leashRadius: 60.0f,
            rotationDegrees: 0f,
            radius: 60.0f,
            arcDegrees: 24f);

        // Boss 3: Void Torrent linear AOE tank buster
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.VoidTorrent,
            width: 7f,
            length: 60f,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            () => ArenaCenter.Albion,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            () => ArenaCenter.GalateaMagna,
            outerRadius: 90.0f,
            innerRadius: 19f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            () => ArenaCenter.Cagnazzo,
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
        /// Boss 1: Albion.
        /// </summary>
        public const uint Albion = 11992;

        /// <summary>
        /// Boss 1: Albion > Wild Beasts.
        /// </summary>
        public const uint WildBeasts = 12060;

        /// <summary>
        /// Boss 1: Albion > Wild Beasts Dummy.
        /// </summary>
        public const uint WildBeastsDummy = 108;

        /// <summary>
        /// Boss 2: Galatea Magna.
        /// </summary>
        public const uint GalateaMagna = 10308;

        /// <summary>
        /// Boss 3: Cagnazzo.
        /// </summary>
        public const uint Cagnazzo = 11995;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss 1: Albion.
        /// </summary>
        public static readonly Vector3 Albion = new(23.97186f, 386.0476f, -744.0146f);

        /// <summary>
        /// Boss 2: Galatea Magna.
        /// </summary>
        public static readonly Vector3 GalateaMagna = new(350f, 34f, -394f);

        /// <summary>
        /// Boss 3: Cagnazzo.
        /// </summary>
        public static readonly Vector3 Cagnazzo = new(-250f, -173f, 130f);
    }

    private static class MechanicLocation
    {
        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Tenebrism Tower, North.
        /// </summary>
        public static readonly Vector3 TenebrismTowerNorth = new(350.00f, 34.00f, -404.00f);

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Tenebrism Tower, South.
        /// </summary>
        public static readonly Vector3 TenebrismTowerSouth = new(350f, 34.00f, -384.00f);

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Tenebrism Tower, East.
        /// </summary>
        public static readonly Vector3 TenebrismTowerEast = new(360.00f, 34.00f, -394.00f);

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Tenebrism Tower, West.
        /// </summary>
        public static readonly Vector3 TenebrismTowerWest = new(340.00f, 34.00f, -394.00f);
    }

    private static class EnemyAura
    {
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Icy Throes.
        ///
        /// Spread AOE targeting players. 6 yalm radius.
        /// </summary>
        public const uint IcyThroesPlayers = 31363;

        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Icy Throes.
        ///
        /// Spread AOE targeting ground via self-targeting helper NPCs. 6 yalm radius.
        /// </summary>
        public const uint IcyThroesGround = 32783;

        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Roar of Albion.
        ///
        /// Room wide AOE. Hide behind rocks.
        /// </summary>
        public const uint RoarOfAlbion = 31364;

        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Albion's Embrace.
        ///
        /// TODO.
        /// </summary>
        public const uint AlbionsEmbrace = 31365;

        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Right Slam.
        ///
        /// TODO.
        /// </summary>
        public const uint RightSlam = 32813;

        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Left Slam.
        ///
        /// TODO.
        /// </summary>
        public const uint LeftSlam = 32814;

        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Knock on Ice.
        ///
        /// TODO.
        /// </summary>
        public const uint KnockOnIce = 31358;

        /// <summary>
        /// <see cref="EnemyNpc.Albion"/>'s Icebreaker.
        ///
        /// TODO.
        /// </summary>
        public const uint Icebreaker = 31361;

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Waning Cycle.
        ///
        /// Donut into Circle AOE.
        /// </summary>
        public const uint WaningCycleDonut = 32622;

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Waning Cycle.
        ///
        /// Donut into Circle AOE.
        /// </summary>
        public const uint WaningCycleCircle = 32624;

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Waxing Cycle.
        ///
        /// Circle into Donut AOE.
        /// </summary>
        public const uint WaxingCycleDonut = 31379;

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Waxing Cycle.
        ///
        /// Circle into Donut AOE.
        /// </summary>
        public const uint WaxingCycleCircle = 31377;

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Soul Scythe.
        ///
        /// Ground-targeted Circle AOE.
        /// </summary>
        public const uint SoulScythe = 31386;

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Scarecrow Chase (Dummy).
        ///
        /// Dummy cast for <see cref="ScarecrowChase"/>. Useful for pre-positioning.
        /// </summary>
        public const uint ScarecrowChaseDummy = 31387;

        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Scarecrow Chase.
        ///
        /// Cross AOE.
        /// </summary>
        public const uint ScarecrowChase = 32703;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Antediluvian.
        ///
        /// Four water balls fall from the ceiling.
        /// </summary>
        public const uint Antediluvian = 31120;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Body Slam.
        ///
        /// Make small donut to avoid being pushed into poison water.
        /// </summary>
        public const uint BodySlamCircle = 31123;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Hydrofall.
        ///
        /// Spread AOE targeting players. 6 yalm radius.
        /// </summary>
        public const uint Hydrofall = 31376;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Hydrovent.
        ///
        /// Ground-targeted circle AOE. 6 yalm radius.
        /// </summary>
        public const uint Hydrovent = 31136;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Hydraulic Ram.
        ///
        /// Multiple NPCs creating line AOEs.
        /// </summary>
        public const uint HydraulicRam = 32692;

        public const uint HydraulicRam1 = 32693;

        public const uint HydraulicRam2 = 32695;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Void Miasma.
        ///
        /// Cone AOE from each purple ball towards each purple tether.
        /// </summary>
        public const uint VoidMiasma = 0x7FB3;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Lifescleaver.
        ///
        /// Alternating cone AOE from center -> outward. 22.5 degrees each.
        /// </summary>
        public const uint Lifescleaver = 0x7989;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Void Torrent.
        ///
        /// Line AOE tank buster. Need to get out of the front if not tank.
        /// </summary>
        public const uint VoidTorrent = 31118;
    }

    private static class PartyAura
    {
        /// <summary>
        /// <see cref="EnemyNpc.GalateaMagna"/>'s Tenebrism > Glassy-Eyed debuff.
        ///
        /// AOE gaze attack from each party member when debuff ends.
        /// </summary>
        public const uint GlassyEyed = 3511;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Cursed Tide > Neap Tide.
        ///
        /// AOE circle targeting party members when debuff ends. Spread.
        /// </summary>
        public const uint NeapTide = 3329;

        /// <summary>
        /// <see cref="EnemyNpc.Cagnazzo"/>'s Cursed Tide > Spring Tide.
        ///
        /// AOE stack targeting one party member when debuff ends. Stack.
        /// </summary>
        public const uint SpringTide = 3328;
    }
}
