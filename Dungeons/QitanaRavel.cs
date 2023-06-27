using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
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
/// Lv. 75: The Qitana Ravel dungeon logic.
/// </summary>
public class QitanaRavel : AbstractDungeon
{
    private static DateTime heatUpLeftTimestamp = DateTime.MinValue;

    private static DateTime heatUpRightTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheQitanaRavel;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheQitanaRavel;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.WrathoftheRonka, EnemyAction.ConfessionofFaith, EnemyAction.HeavingBreath };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1 Lozatl's Fury Right
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDivineThreshold,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.LozatlsFuryRight,
            leashPointProducer: () => ArenaCenter.Lozatl,
            leashRadius: 40.0f,
            rotationDegrees: -90.0f,
            radius: 60.0f,
            arcDegrees: 180.0f);

        // Boss 1 Lozatl's Fury Left
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDivineThreshold,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.LozatlsFuryLeft,
            leashPointProducer: () => ArenaCenter.Lozatl,
            leashRadius: 40.0f,
            rotationDegrees: 90.0f,
            radius: 60.0f,
            arcDegrees: 180.0f);

        // Boss 2 Towerfall
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ShadowedHollow,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.Towerfall,
            leashPointProducer: () => ArenaCenter.Lozatl,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 60.0f,
            arcDegrees: 35.0f);

        // Boss 3, Poison puddles
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSongofOxGatorl,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.PoisonPuddle,
            radiusProducer: eo => 6.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDivineThreshold,
            () => ArenaCenter.Lozatl,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ShadowedHollow,
            () => ArenaCenter.Batsquatch,
            outerRadius: 90.0f,
            innerRadius: 14f,
            priority: AvoidancePriority.High);

        // No third arena, as this boss arena is a square
        /*
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSongofOxGatorl,
            () => ArenaCenter.Eros,
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

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.TheDivineThreshold:
                result = await HandleLozatlAsync();
                break;
            case SubZoneId.ShadowedHollow:
                result = await HandleBatsquatchAsync();
                break;
            case SubZoneId.TheSongofOxGatorl:
                result = await HandleErosAsync();
                break;
        }

        return false;
    }

    private async Task<bool> HandleLozatlAsync()
    {
        if (EnemyAction.HeatUpLeft.IsCasting())
        {
            // Placing a donut on the right side so we move into that area when the left statue casts
            heatUpLeftTimestamp = DateTime.Now;
            Stopwatch heatUpLeftTimer = new();
            heatUpLeftTimer.Restart();

            AvoidanceHelpers.AddAvoidDonut(
                () => heatUpLeftTimer.IsRunning && heatUpLeftTimer.ElapsedMilliseconds < EnemyAction.HeatUpLeftDuration,
                () => ArenaCenter.LozatlRightSide,
                outerRadius: 90.0f,
                innerRadius: 19.0f,
                priority: AvoidancePriority.High);
        }

        if (EnemyAction.HeatUpRight.IsCasting())
        {
            // Placing a donut on the right side so we move into that area when the right statue casts
            heatUpRightTimestamp = DateTime.Now;
            Stopwatch heatUpRightTimer = new();
            heatUpRightTimer.Restart();

            AvoidanceHelpers.AddAvoidDonut(
                () => heatUpRightTimer.IsRunning && heatUpRightTimer.ElapsedMilliseconds < EnemyAction.HeatUpRightDuration,
                () => ArenaCenter.LozatlLeftSide,
                outerRadius: 90.0f,
                innerRadius: 19.0f,
                priority: AvoidancePriority.High);
        }

        return false;
    }

    private async Task<bool> HandleBatsquatchAsync()
    {
        return false;
    }

    private async Task<bool> HandleErosAsync()
    {
        BattleCharacter ErosNPC = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Eros)
            .FirstOrDefault(bc => bc.IsTargetable && bc.IsValid);

        if (EnemyAction.HoundoutofHeaven.IsCasting() && ErosNPC.TargetGameObject == Core.Me)
        {
            AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
                condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSongofOxGatorl,
                objectSelector: bc => bc.CastingSpellId == 15514,
                radiusProducer: eo => 25f,
                priority: AvoidancePriority.High));
        }

        if (EnemyAction.ConfessionofFaithSpread.IsCasting())
        {
            await MovementHelpers.Spread(EnemyAction.ConfessionofFaithSpreadDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Lozatl.
        /// </summary>
        public const uint Lozatl = 8231;

        /// <summary>
        /// Second Boss: Batsquatch.
        /// </summary>
        public const uint Batsquatch = 8232;

        /// <summary>
        /// Final Boss: Eros.
        /// </summary>
        public const uint Eros = 8233;

        /// <summary>
        /// Final Boss: Poison Puddle.
        /// </summary>
        public const uint PoisonPuddle = 2004780;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Lozatl.
        /// </summary>
        public static readonly Vector3 Lozatl = new(0, 5f, 315);

        /// <summary>
        /// First Boss: Lozatl.
        /// </summary>
        public static readonly Vector3 LozatlRightSide = new(19.5f, 5f, 316.5f);

        /// <summary>
        /// First Boss: Lozatl.
        /// </summary>
        public static readonly Vector3 LozatlLeftSide = new(-19.5f, 5f, 315f);

        /// <summary>
        /// Second Boss: Batsquatch.
        /// </summary>
        public static readonly Vector3 Batsquatch = new(62f, -21f, -35.5f);

        /// <summary>
        /// Third Boss: Eros.
        /// </summary>
        public static readonly Vector3 Eros = new(17f, -77f, -538f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Trash
        /// Wrath of the Ronka
        /// Lazers come out of the statues on the wall
        /// </summary>
        public const uint WrathoftheRonka = 17223;

        /// <summary>
        /// Lozatl
        /// Lozatl's Fury
        /// Right side 180 cone
        /// </summary>
        public const uint LozatlsFuryRight = 15503;

        /// <summary>
        /// Lozatl
        /// Lozatl's Fury
        /// Left side 180 cone
        /// </summary>
        public const uint LozatlsFuryLeft = 15504;

        /// <summary>
        /// Lozatl
        /// Heat Up
        /// Left side status
        /// </summary>
        public static readonly HashSet<uint> HeatUpLeft = new() { 15501 };

        public static readonly int HeatUpLeftDuration = 22_000;

        /// <summary>
        /// Lozatl
        /// Heat Up
        /// Right side status
        /// </summary>
        public static readonly HashSet<uint> HeatUpRight = new() { 15502 };

        public static readonly int HeatUpRightDuration = 22_000;

        /// <summary>
        /// Batsquatch
        /// Towerfall
        /// Very small cone to dodge the fall
        /// </summary>
        public const uint Towerfall = 15512;

        /// <summary>
        /// Eros
        /// Confession of Faith
        /// Stack
        /// </summary>
        public const uint ConfessionofFaith = 15525;

        /// <summary>
        /// Eros
        /// Confession of Faith
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> ConfessionofFaithSpread = new() { 15523 };
        public static readonly int ConfessionofFaithSpreadDuration = 6_000;

        /// <summary>
        /// Eros
        /// Heaving Breath
        /// Stack
        /// </summary>
        public const uint HeavingBreath = 15520;

        /// <summary>
        /// Eros
        /// Hound out of Heaven
        /// Run away from boss, if you're the target
        /// </summary>
        public static readonly HashSet<uint> HoundoutofHeaven = new() { 15514 };
    }

    private static class TankBusters
    {
        /// <summary>
        /// Lozatl
        /// Stonefist
        /// </summary>
        public const uint Stonefist = 15497;

        /// <summary>
        /// Eros
        /// Rend
        /// </summary>
        public const uint Rend = 15513;
    }
}
