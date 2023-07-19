using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Localization;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 83: The Tower of Babil dungeon logic.
/// </summary>
public class TowerOfBabil : AbstractDungeon
{
    private static DateTime? animaLowerChaseStartTimestamp = null;
    private static DateTime? animaLowerChaseEndTimestamp = null;

    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheTowerOfBabil;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheTowerOfBabil;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.ShockingForce };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        if (lastSubZoneId != currentSubZoneId)
        {
            Logger.Information(Translations.SUBZONE_CHANGED_CLEARING_AVOIDS, currentSubZoneId);

            AvoidanceManager.RemoveAllAvoids(avoidInfo => true);
            AvoidanceManager.ResetNavigation();
        }

        bool result = currentSubZoneId switch
        {
            SubZoneId.MagitekServicing => await HandleBarnabasAsync(),
            SubZoneId.MartialConditioning => await HandleLugaeAsync(),
            SubZoneId.TheIronWomb => await HandleAnimaUpperAsync(),
            SubZoneId.AnimasDimension => await HandleAnimaLowerAsync(),
            _ => await HandleTrashAsync(),
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Barnabas.
    /// </summary>
    private Task<bool> HandleBarnabasAsync()
    {
        // Override SideStep for this fight
        SidestepPlugin.Enabled = false;

        if (lastSubZoneId is not SubZoneId.MagitekServicing)
        {
            uint currentSubZoneId = WorldManager.SubZoneId;
            Logger.Information(Translations.SUBZONE_CHANGED_ADDING_AVOIDS, (SubZoneId)currentSubZoneId);

            // Boss Arena
            AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat,
                () => ArenaCenter.Barnabas,
                outerRadius: 90.0f,
                innerRadius: 14.5f,
                priority: AvoidancePriority.High);

            // Ground and Pound
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.GroundAndPound,
                width: 6.0f,
                length: 60f);

            // Electromagnetic Release (Line)
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.ElectromagneticReleaseLine,
                width: 6.0f,
                length: 60.0f);

            // Electromagnetic Release (Circle)
            AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
                condition: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.ElectromagneticReleaseCircle,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius));

            // TODO: Update Positive/Negative logic when head markers are available.
            // We can infer the boss's polarity by action ID, but can't figure out our own polarity without head markers.
            // For now, always position outwards because the inner AOE is much more dangerous than the outside DOT.

            // Dynamic Pound (Negative)
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.DynamicPoundNegative,
                width: 6.0f + 18.0f,
                length: 60.0f);

            // Dynamic Pound (Positive)
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.DynamicPoundPositive,
                width: 6.0f + 18.0f,
                length: 60.0f);

            // Dynamic Scrapline (Negative)
            AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
                condition: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.DynamicScraplineNegative,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius + 5.0f));

            // Dynamic Scrapline (Positive)
            AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
                condition: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.DynamicScraplinePositive,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius + 5.0f));

            // Rolling Scrapline
            AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
                condition: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.RollingScrapline,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius));

            // Thundercall / Thunderball
            // Instead of waiting for Thunderballs to cast Shock, pre-position away from them ASAP.
            AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
                condition: () => Core.Player.InCombat,
                objectSelector: bc => bc.NpcId == EnemyNpc.Thunderball && bc.IsVisible,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius));
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Boss 2: Lugae.
    /// </summary>
    private Task<bool> HandleLugaeAsync()
    {
        // LUGAE (B2)
        // Magitek Missile     25334
        // Magitek Ray         25340
        // Magitek Explosive   25336

        // Override SideStep for this fight
        SidestepPlugin.Enabled = false;

        if (lastSubZoneId is not SubZoneId.MartialConditioning)
        {
            uint currentSubZoneId = WorldManager.SubZoneId;
            Logger.Information(Translations.SUBZONE_CHANGED_ADDING_AVOIDS, (SubZoneId)currentSubZoneId);

            // Boss Arena
            AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat,
                () => ArenaCenter.Lugae,
                outerRadius: 90.0f,
                innerRadius: 19.0f,
                priority: AvoidancePriority.High);

            // Magitek Missile
            AvoidanceManager.AddAvoidLocation(
                canRun: () => Core.Player.InCombat,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius,
                locationProducer: bc => bc.SpellCastInfo?.CastLocation ?? bc.TargetGameObject.Location,
                collectionProducer: () => GameObjectManager.GetObjectsOfType<BattleCharacter>()
                    .Where(bc => bc.CastingSpellId == EnemyAction.SurfaceMissile));

            // Magitek Ray
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.MagitekRay,
                width: 6.0f,
                length: 60f);

            // Pre-position center for Downpour, Magitek Chakram.
            // Don't draw during recasts with Chakrams on the field so the Minimum donut isn't blocked.
            AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat
                    && GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Lugae)
                        .Any(bc => bc.CastingSpellId is EnemyAction.Downpour or EnemyAction.MagitekChakram)
                    && !GameObjectManager.GetObjectsByNPCId(EnemyNpc.MagitekChakram).Any(obj => obj.IsVisible),
                () => ArenaCenter.Lugae,
                outerRadius: 90.0f,
                innerRadius: 3.0f,
                priority: AvoidancePriority.High);

            // Downpour / Breathless / Frog Transformation
            AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat && Core.Player.HasAura(PartyAura.Breathless),
                () => MechanicLocation.LugaeFrog,
                outerRadius: 90.0f,
                innerRadius: 3.0f,
                priority: AvoidancePriority.High);

            AvoidanceManager.AddAvoidLocation(
               canRun: () => Core.Player.InCombat && Core.Player.HasAura(PartyAura.Toad),
               radiusProducer: location => 6f,
               locationProducer: location => location,
               collectionProducer: () => new Vector3[] { MechanicLocation.LugaeFrog, });

            AvoidanceManager.AddAvoidLocation(
                canRun: () => Core.Player.InCombat
                    && (Core.Player.HasAura(PartyAura.Breathless) || Core.Player.HasAura(PartyAura.Toad)),
                radiusProducer: location => 6f,
                locationProducer: location => location,
                collectionProducer: () => new Vector3[] { MechanicLocation.LugaeShrink, });

            // Magitek Chakram / Minimum / Shrink Transformation
            AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat && !Core.Player.HasAura(PartyAura.Minimum)
                    && GameObjectManager.GetObjectsByNPCId(EnemyNpc.MagitekChakram).Any(obj => obj.IsVisible),
                () => MechanicLocation.LugaeShrink,
                outerRadius: 90.0f,
                innerRadius: 3.0f,
                priority: AvoidancePriority.High);

            AvoidanceManager.AddAvoidLocation(
                canRun: () => Core.Player.InCombat && Core.Player.HasAura(PartyAura.Minimum)
                     && GameObjectManager.GetObjectsByNPCId(EnemyNpc.MagitekChakram).Any(obj => obj.IsVisible),
                radiusProducer: location => 6f,
                locationProducer: location => location,
                collectionProducer: () => new Vector3[] { MechanicLocation.LugaeFrog, });

            AvoidanceManager.AddAvoidLocation(
                canRun: () => Core.Player.InCombat
                    && GameObjectManager.GetObjectsByNPCId(EnemyNpc.MagitekChakram).Any(obj => obj.IsVisible),
                radiusProducer: location => 6f,
                locationProducer: location => location,
                collectionProducer: () => new Vector3[] { MechanicLocation.LugaeFrog, });

            // Magitek Explosive
            // Instead of waiting for Magitek Explosives to cast Explosion, pre-position away from them ASAP.
            AvoidanceHelpers.AddAvoidCross<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.NpcId == EnemyNpc.MagitekExplosive && bc.IsVisible,
                thickness: 8.5f,
                length: 60.0f);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Boss 3a: Anima, upper half.
    /// </summary>
    private Task<bool> HandleAnimaUpperAsync()
    {
        // Override SideStep for this fight
        SidestepPlugin.Enabled = false;

        if (lastSubZoneId is not SubZoneId.TheIronWomb)
        {
            uint currentSubZoneId = WorldManager.SubZoneId;
            Logger.Information(Translations.SUBZONE_CHANGED_ADDING_AVOIDS, (SubZoneId)currentSubZoneId);

            // Boss Arena, front edge blocker
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.NpcId == EnemyNpc.AnimaUpper && bc.IsTargetable,
                width: 180.0f,
                length: 90.0f,
                yOffset: -90.0f);

            // Boss Arena, unused half blocker
            AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
                () => Core.Player.InCombat,
                objectSelector: bc => bc.NpcId == EnemyNpc.AnimaUpper && bc.IsTargetable,
                outerRadius: 90.0f,
                innerRadius: 22.0f,
                priority: AvoidancePriority.High);

            // Lunar Nail / Phantom Pain
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.PhantomPain,
                width: 21.0f,
                length: 21.0f,
                yOffset: -10.5f);

            // Mega Graviton, pre-position middle
            AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat
                    && GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.AnimaUpper)
                        .Any(bc => bc.CastingSpellId == EnemyAction.MegaGraviton),
                () => ArenaCenter.AnimaUpper,
                outerRadius: 90.0f,
                innerRadius: 3.0f,
                priority: AvoidancePriority.High);

            // Black Hole / Aetherial Pull
            AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
                condition: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.AetherialPull && bc.SpellCastInfo.TargetId == Core.Player.ObjectId,
                radiusProducer: bc => 34.0f));

            // Pater Patriae
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.PaterPatriae
                    && !GameObjectManager.GetObjectsByNPCId(EnemyNpc.MegaGraviton).Any(obj => obj.IsVisible),
                width: 7.0f,
                length: 60.0f,
                priority: AvoidancePriority.High);

            // Boundless Pain / Area of Influence Up
            AvoidanceManager.AddAvoidObject<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.NpcId == EnemyNpc.AnimaUpper && bc.HasAura(EnemyAura.AreaOfInfluenceUp),
                radiusProducer: bc => 18.0f);

            // Erupting Pain
            AvoidanceManager.AddAvoidObject<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.EruptingPain && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius,
                locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Boss 3b: Anima, lower half.
    /// </summary>
    private Task<bool> HandleAnimaLowerAsync()
    {
        // Override SideStep for this fight
        SidestepPlugin.Enabled = false;

        if (lastSubZoneId is not SubZoneId.AnimasDimension)
        {
            uint currentSubZoneId = WorldManager.SubZoneId;
            Logger.Information(Translations.SUBZONE_CHANGED_ADDING_AVOIDS, (SubZoneId)currentSubZoneId);

            // Boss Arena, front edge blocker
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.NpcId == EnemyNpc.AnimaLower && bc.IsTargetable,
                width: 180.0f,
                length: 90.0f,
                yOffset: -90.0f);

            // Boss Arena, unused half blocker
            AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
                () => Core.Player.InCombat,
                objectSelector: bc => bc.NpcId == EnemyNpc.AnimaLower && bc.IsTargetable,
                outerRadius: 90.0f,
                innerRadius: 22.0f,
                priority: AvoidancePriority.High);

            // Obliviating Claw
            AvoidanceManager.AddAvoidObject<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.ObliviatingClaw,
                radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius);

            // Charnel Claw
            AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
                canRun: () => Core.Player.InCombat,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.CharnelClaw
                    && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 4_000,
                width: 6.0f,
                length: 60.0f);

            // Obliviating Claw Chase / Coffin Scratch Start
            AvoidanceHelpers.AddAvoidCross<BattleCharacter>(
                canRun: () => Core.Player.InCombat && DateTime.Now < animaLowerChaseStartTimestamp,
                objectSelector: bc => bc.CastingSpellId == EnemyAction.ObliviatingClawChaseStart,
                locationProducer: bc => MechanicLocation.AnimaLowerChaseStartBlocker,
                thickness: 36.0f,
                length: 60.0f);

            // Obliviating Claw Chase / Coffin Scratch End
            AvoidanceHelpers.AddAvoidCross<BattleCharacter>(
                canRun: () => Core.Player.InCombat && DateTime.Now < animaLowerChaseEndTimestamp,
                objectSelector: bc => true,
                locationProducer: bc => MechanicLocation.AnimaLowerChaseEndBlocker,
                rotationProducer: bc => 0f,
                thickness: 36.0f,
                length: 60.0f);

            animaLowerChaseStartTimestamp = null;
            animaLowerChaseEndTimestamp = null;
        }

        // Don't take this as a good example of chaining mechanics; it's ugly and can't repeat.
        // This is a horrific hack borne of a nonsense need to do everything by drawing avoids,
        // but also a need to move on to other tasks after much delay here.
        // Maybe we'll eventually have better APIs and timelines or at least self-contained timers.
        // TODO: Detect head marker 0xC5 to decide if we even need to make this run.
        if (animaLowerChaseStartTimestamp is null
            && GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Any(bc => bc.CastingSpellId == EnemyAction.ObliviatingClawChaseStart))
        {
            const int coffinScratchPlaceDuration = 5_000;
            animaLowerChaseStartTimestamp = DateTime.Now.AddMilliseconds(coffinScratchPlaceDuration);
            Logger.Information($"Time to start Anima Chase: {animaLowerChaseStartTimestamp}");
        }

        if (animaLowerChaseEndTimestamp is null && animaLowerChaseStartTimestamp < DateTime.Now)
        {
            const int coffinScratchChaseDuration = 12_000;
            animaLowerChaseEndTimestamp = DateTime.Now.AddMilliseconds(coffinScratchChaseDuration);
            Logger.Information($"Time to end Anima Chase: {animaLowerChaseEndTimestamp}");
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Generic handling for all non-boss zones.
    /// </summary>
    private Task<bool> HandleTrashAsync()
    {
        // Avoid Claw Game in final zone only if in combat, otherwise bot gets stuck frequently
        SidestepPlugin.Enabled = Core.Me.InCombat;

        return Task.FromResult(false);
    }

    private static class EnemyNpc
    {
        public const uint Barnabas = 10279;
        public const uint Thunderball = 10280;

        public const uint Lugae = 10282;
        public const uint MagitekChakram = 10283;
        public const uint MagitekExplosive = 10284;

        public const uint AnimaUpper = 10285;
        public const uint MegaGraviton = 10287;

        public const uint AnimaLower = 10288;
        public const uint IronNail = 10289;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss 1: Barnabas.
        /// </summary>
        public static readonly Vector3 Barnabas = new(-300f, -175f, 71f);

        /// <summary>
        /// Boss 2: Lugae.
        /// </summary>
        public static readonly Vector3 Lugae = new(221f, 0.95f, 306f);

        /// <summary>
        /// Boss 3a: Anima, upper half.
        /// </summary>
        public static readonly Vector3 AnimaUpper = new(0f, 480f, -180f);

        /// <summary>
        /// Boss 3b: Anima, lower half.
        /// </summary>
        public static readonly Vector3 AnimaLower = new(0f, 120, -400f);
    }

    private static class MechanicLocation
    {
        public static readonly Vector3 LugaeShrink = new(229f, 0.95f, 306f);
        public static readonly Vector3 LugaeFrog = new(213f, 0.95f, 306f);

        public static readonly Vector3 AnimaLowerChaseStartBlocker = new(2f, 120, -387f);
        public static readonly Vector3 AnimaLowerChaseEndBlocker = new(-2f, 120, -387f);
    }

    private static class EnemyAura
    {
        /// <summary>
        /// <see cref="EnemyNpc.AnimaUpper"/>'s Boundless Pain / Area of Influence Up.
        ///
        /// Boundless Pain is a circle AOE expanding from the room's center.
        /// Size depends on this buff's stacks, up to 12.
        /// </summary>
        public const uint AreaOfInfluenceUp = 0x6D5;
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Ground and Pound.
        ///
        /// Simple line AOE. 6 yalms wide.
        /// </summary>
        public const uint GroundAndPound = 0x62EA;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Electromagnetic Release (Line).
        ///
        /// Simple line AOE paired with both Dynamic Pounds. 6 yalms wide.
        /// </summary>
        public const uint ElectromagneticReleaseLine = 0x62EF;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Electromagnetic Release (Circle).
        ///
        /// Self-targeted circle AOE paired with both Dynamic Scraplines. 8 yalms radius.
        /// </summary>
        public const uint ElectromagneticReleaseCircle = 0x62F1;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Dynamic Pound (Negative).
        ///
        /// Magnetic line AOE splitting room into east and west halves.
        /// Pulls players with + charge and pushes players with - charge.
        /// </summary>
        public const uint DynamicPoundNegative = 0x6245;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Dynamic Pound (Positive).
        ///
        /// Magnetic line AOE splitting room into east and west halves.
        /// Pulls players with - charge and pushes players with + charge.
        /// </summary>
        public const uint DynamicPoundPositive = 0x62EE;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Shocking Force.
        ///
        /// Party stack targeting a player. 6 yalms radius.
        /// </summary>
        public const uint ShockingForce = 0x62EC;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Dynamic Scrapline (Negative).
        ///
        /// Magnetic self-targeted circle AOE. 8 yalm radius, 6 yalm movement.
        /// Pulls players with + charge and pushes players with - charge.
        /// </summary>
        public const uint DynamicScraplineNegative = 0x6246;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Dynamic Scrapline (Positive).
        ///
        /// Magnetic self-targeted circle AOE. 8 yalm radius, 6 yalm movement.
        /// Pulls players with - charge and pushes players with + charge.
        /// </summary>
        public const uint DynamicScraplinePositive = 0x62F0;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Rolling Scrapline.
        ///
        /// Self-targeted circle AOE paired with <see cref="EnemyAction.Thundercall"/>. 8 yalm radius.
        /// </summary>
        public const uint RollingScrapline = 0x62EB;

        /// <summary>
        /// <see cref="EnemyNpc.Barnabas"/>'s Thundercall.
        ///
        /// Dummy spell that summons <see cref="EnemyNpc.Thunderball"/>.
        /// </summary>
        public const uint Thundercall = 0x62ED;

        /// <summary>
        /// <see cref="EnemyNpc.Thunderball"/>'s Shock.
        ///
        /// Self-targeted circle AOE. 8 yalm radius.
        /// </summary>
        public const uint Shock = 0x62F2;

        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Magitek Missile.
        ///
        /// Dummy cast for <see cref="EnemyAction.SurfaceMissile"/>.
        /// </summary>
        public const uint MagitekMissile = 0x62F6;

        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Surface Missile.
        ///
        /// Ground-targeted circle AOE. 6 yalm radius.
        /// </summary>
        public const uint SurfaceMissile = 0x62F7;

        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Magitek Ray.
        ///
        /// Simple line AOE. 6 yalms wide.
        /// </summary>
        public const uint MagitekRay = 0x62FC;

        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Magitek Chakram.
        ///
        /// Dummy cast for Chakram phase with <see cref="EnemyAction.MightyBlow"/>.
        /// Useful signal to pre-position center for <see cref="PartyAura.Minimum"/>.
        /// </summary>
        public const uint MagitekChakram = 0x62F3;

        /// <summary>
        /// <see cref="EnemyNpc.MagitekChakram"/>'s Mighty Blow.
        ///
        /// Simple line AOE.
        /// </summary>
        public const uint MightyBlow = 0x62F4;

        /// <summary>
        /// <see cref="EnemyNpc.MagitekExplosive"/>'s Explosion.
        ///
        /// Simple cross-lines AOE. 4 yalms wide.
        /// </summary>
        public const uint Explosion = 0x62F9;

        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Downpour.
        ///
        /// Dummy cast for Frog phase.
        /// Useful signal to pre-position center for <see cref="PartyAura.Breathless"/>.
        /// </summary>
        public const uint Downpour = 0x62F5;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaUpper"/>'s Phantom Pain (Dummy).
        ///
        /// Dummy cast for <see cref="EnemyAction.PhantomPain"/>.
        /// </summary>
        public const uint PhantomPainDummy = 0x52BE;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaUpper"/>'s Phantom Pain.
        ///
        /// Square AOEs placed between tethered Lunar Nails. 20x20 yalms.
        /// </summary>
        public const uint PhantomPain = 0x62FF;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaUpper"/>'s Mega Graviton.
        ///
        /// Summons <see cref="EnemyNpc.MegaGraviton"/> for black hole mechanic.
        /// </summary>
        public const uint MegaGraviton = 0x6300;

        /// <summary>
        /// <see cref="EnemyNpc.MegaGraviton"/>'s Aetherial Pull.
        ///
        /// Player-targeted tether that pulls 36 yalms and does damage if too close.
        /// </summary>
        public const uint AetherialPull = 0x6301;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaUpper"/>'s Pater Patriae (Dummy).
        ///
        /// Dummy cast for <see cref="EnemyAction.PaterPatriae"/>.
        /// </summary>
        public const uint PaterPatriaeDummy = 0x6306;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaUpper"/>'s Pater Patriae.
        ///
        /// Simple line AOE. 6 yalms wide.
        /// </summary>
        public const uint PaterPatriae = 0x5E68;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaUpper"/>'s Erupting Pain.
        ///
        /// Player-targeted circle AOE.
        /// </summary>
        public const uint EruptingPain = 0x6308;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaLower"/>'s Obliviating Claw (Dummy).
        ///
        /// Dummy cast for <see cref="EnemyAction.ObliviatingClaw"/>.
        /// </summary>
        public const uint ObliviatingClawDummy = 0x630B;

        /// <summary>
        /// <see cref="EnemyNpc.IronNail"/>'s Obliviating Claw.
        ///
        /// Simple circle AOE that spawns <see cref="EnemyNpc.IronNail"/>. 3 yalms radius.
        /// </summary>
        public const uint ObliviatingClaw = 0x630C;

        /// <summary>
        /// <see cref="EnemyNpc.IronNail"/>'s Charnel Claw.
        ///
        /// Simple line AOE. 5 yalms width.
        /// </summary>
        public const uint CharnelClaw = 0x630D;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaLower"/>'s Obliviating Claw (Chase Pre-Cast).
        ///
        /// Dummy cast before <see cref="EnemyAction.CoffinScratchChaseStart"/>.
        /// Useful to pre-position before the chase-AOE begins.
        /// </summary>
        public const uint ObliviatingClawChaseStart = 0x630A;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaLower"/>'s Coffin Scratch (Chase Start).
        ///
        /// Simple circle AOE before starting <see cref="EnemyAction.CoffinScratchChaseHit"/>. Pretend this is 25 yalms radius.
        /// </summary>
        public const uint CoffinScratchChaseStart = 0x630E;

        /// <summary>
        /// <see cref="EnemyNpc.AnimaLower"/>'s Coffin Scratch (Chase Start).
        ///
        /// Simple circle AOE for each hit after <see cref="EnemyAction.CoffinScratchChaseStart"/>. Pretend this is 25 yalms radius.
        /// </summary>
        public const uint CoffinScratchChaseHit = 0x52F7;
    }

    private static class PartyAura
    {
        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Downpour / Breathless.
        ///
        /// Breathless stacks every 2 seconds and kills at 8 stacks.
        /// The mechanic lasts for 20 seconds, so 10 total stacks are possible.
        /// The frog transformation also lasts 20 seconds, or toggle frog on/off
        /// at >= 3 stacks to live while still attacking.
        /// </summary>
        public const uint Breathless = 0xA70;

        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Downpour / Toad.
        /// </summary>
        public const uint Toad = 0xA6F;

        /// <summary>
        /// <see cref="EnemyNpc.Lugae"/>'s Magitek Chakram / Minimum.
        /// </summary>
        public const uint Minimum = 0x9C8;
    }
}
