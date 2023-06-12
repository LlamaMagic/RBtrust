using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 61: The Sirensong Sea dungeon logic.
/// </summary>
public class SirensongSea : AbstractDungeon
{
    private static readonly int ShadowflowDuration = 12_500;
    private static DateTime ShadowflowTimestamp = DateTime.MinValue;

    private static readonly int EnterNightDuration = 15_000;
    private static DateTime EnterNightTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheSirensongSea;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheSirensongSea;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Hydroball, EnemyAction.MorbidAdvance, EnemyAction.MorbidRetreat };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Blue puddles of fire that fall on the player between the first boss and the second
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => WorldManager.ZoneId == (uint)ZoneId.TheSirensongSea,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.BlueFirePuddle,
            radiusProducer: eo => 4.0f,
            priority: AvoidancePriority.High));

        // Boss 2
        // Shadow clones of the boss spawn and do a black puddle AoE. Stay away from them
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GloweringKrautz,
            objectSelector: bc => bc.NpcId == EnemyNpc.TheGroveller,
            radiusProducer: bc => 7.0f,
            priority: AvoidancePriority.High));

        // Blue water puddles during the final boss
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WardensDelight,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.WaterPuddle,
            radiusProducer: eo => 7.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SpaeRock,
            () => ArenaCenter.Lugat,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GloweringKrautz,
            () => ArenaCenter.TheGovernor,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WardensDelight,
            () => ArenaCenter.Lorelei,
            outerRadius: 90.0f,
            innerRadius: 15.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (WorldManager.SubZoneId == (uint)SubZoneId.GloweringKrautz && Core.Player.InCombat)
        {
            BattleCharacter TheGovernorNPC = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.TheGovernor)
                .FirstOrDefault(bc => bc.IsTargetable && bc.IsValid);

            if (EnemyAction.EnterNight.IsCasting() && TheGovernorNPC.TargetGameObject == Core.Me)
            {
                EnterNightTimestamp = DateTime.Now;
                Stopwatch enterNightTimer = new();
                enterNightTimer.Restart();

                AvoidanceHelpers.AddAvoidDonut(
                    () => enterNightTimer.IsRunning && enterNightTimer.ElapsedMilliseconds < EnterNightDuration,
                    () => ArenaCenter.TheGovernorArenaEdge,
                    outerRadius: 40.0f,
                    innerRadius: 3.0F,
                    priority: AvoidancePriority.High);
            }

            if (EnemyAction.Shadowflow.IsCasting() && ShadowflowTimestamp.AddMilliseconds(ShadowflowDuration) < DateTime.Now)
            {
                Vector3 location = TheGovernorNPC.Location;
                uint objectId = TheGovernorNPC.ObjectId;

                ShadowflowTimestamp = DateTime.Now;
                Stopwatch shadowflowTimer = new();
                shadowflowTimer.Restart();

                AvoidanceManager.AddAvoidObject<GameObject>(
                    canRun: () =>
                        shadowflowTimer.IsRunning &&
                        shadowflowTimer.ElapsedMilliseconds < ShadowflowDuration,
                    radius: 6f,
                    unitIds: objectId);

                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        shadowflowTimer.IsRunning &&
                        shadowflowTimer.ElapsedMilliseconds < ShadowflowDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 0f,
                    rotationDegrees: 30f,
                    radius: 30f,
                    arcDegrees: 45f);


                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        shadowflowTimer.IsRunning &&
                        shadowflowTimer.ElapsedMilliseconds < ShadowflowDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 0f,
                    rotationDegrees: 90f,
                    radius: 30f,
                    arcDegrees: 45f);

                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        shadowflowTimer.IsRunning &&
                        shadowflowTimer.ElapsedMilliseconds < ShadowflowDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 0f,
                    rotationDegrees: 150f,
                    radius: 30f,
                    arcDegrees: 45f);

                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        shadowflowTimer.IsRunning &&
                        shadowflowTimer.ElapsedMilliseconds < ShadowflowDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 0f,
                    rotationDegrees: 210f,
                    radius: 30f,
                    arcDegrees: 45f);

                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        shadowflowTimer.IsRunning &&
                        shadowflowTimer.ElapsedMilliseconds < ShadowflowDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 0f,
                    rotationDegrees: 270f,
                    radius: 30f,
                    arcDegrees: 45f);

                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        shadowflowTimer.IsRunning &&
                        shadowflowTimer.ElapsedMilliseconds < ShadowflowDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 0f,
                    rotationDegrees: 330f,
                    radius: 30f,
                    arcDegrees: 45f);
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Lugat.
        /// </summary>
        public const uint Lugat = 6071;

        /// <summary>
        /// The Jane Guy casts the Bluefire puddle, the fire puddle is the remaining affect on the ground
        /// </summary>
        public const uint TheJaneGuy = 6070;

        /// <summary>
        /// Before second boss.
        /// </summary>
        public const uint BlueFirePuddle = 2007809;

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public const uint TheGovernor = 6072;

        /// <summary>
        /// Second Boss: The Groveller.
        /// </summary>
        public const uint TheGroveller = 6073;

        /// <summary>
        /// Final Boss: Lorelei .
        /// </summary>
        public const uint Lorelei = 6074;

        /// <summary>
        /// Before Final boss.
        /// </summary>
        public const uint WaterPuddle = 2007808;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Lugat.
        /// </summary>
        public static readonly Vector3 Lugat = new(-1.791643f, -2.900793f, -215.6073f);

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public static readonly Vector3 TheGovernor = new(-7.938193f, 4.440489f, 79.09968f);

        /// <summary>
        /// Second Boss: The Governor > Arena edge.
        /// </summary>
        public static readonly Vector3 TheGovernorArenaEdge = new(8.985318f, 4.437799f, 70.16875f);

        /// <summary>
        /// Third Boss: Lorelei.
        /// </summary>
        public static readonly Vector3 Lorelei = new(-44.54654f, 7.751197f, 465.0925f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Lugat
        /// Hydroball
        /// Stack
        /// </summary>
        public const uint Hydroball = 8023;

        /// <summary>
        /// The Governor
        /// Enter Night
        /// Move to edge of arena to break tether
        /// </summary>
        public static readonly HashSet<uint> EnterNight = new() { 8032 };

        /// <summary>
        /// The Governor
        /// Shadowflow
        /// relative to boss facing, that should be cones aiming at 30, 90, 120, 210, 300 going clockwise
        /// </summary>
        public static readonly HashSet<uint> Shadowflow = new() { 8030 };

        /// <summary>
        /// Lorelei
        /// Morbid Retreat
        /// Move to NPCs so you hopefully don't walk through as much bad
        /// </summary>
        public const uint MorbidRetreat = 8038;

        /// <summary>
        /// Lorelei
        /// Morbid Advance
        /// Move to NPCs so you hopefully don't walk through as much bad
        /// </summary>
        public const uint MorbidAdvance = 8037;
    }
}
