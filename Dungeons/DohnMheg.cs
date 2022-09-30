using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 73: Dohn Mheg dungeon logic.
/// </summary>
public class DohnMheg : AbstractDungeon
{
    private const int LordOfLingeringGaze = 8141;
    private const int Griaule = 8143;
    private const int PaintedSapling = 8144;
    private const int LordOfLengthsomeGait = 8146;
    private const int LiarsLyre = 8958;

    private const int ImpChoir = 13552;
    private const int ToadChoir = 13551;
    private const int Finale = 15723;

    private const int LaughingLeapDuration = 30_000;
    private const int FodderDuration = 13_000;
    private const float FodderDistance = 0.5f;

    private static readonly Vector3 LordOfLingeringGazeArenaCenter = new(0f, 6.85f, 30.16f);
    private static readonly Vector3 GriauleArenaCenter = new(7f, 23f, -339f);
    private static readonly Vector3 LordOfLengthsomeGaitArenaCenter = new(-128f, -144.5f, -244f);

    private readonly Stopwatch laughingLeapSw = new();

    private readonly HashSet<uint> laughingLeap = new()
    {
        8852, 8840,
    };

    private readonly List<Vector3> tightRopeWalkPoints = new()
    {
        new Vector3(-142.8355f, -144.5264f, -232.6624f),
        new Vector3(-140.8284f, -144.5366f, -246.1443f),
        new Vector3(-130.1889f, -144.5366f, -242.3840f),
        new Vector3(-114.4550f, -144.5366f, -244.2632f),
        new Vector3(-125.6857f, -144.5238f, -249.2640f),
        new Vector3(-122.5055f, -144.5192f, -258.3726f),
        new Vector3(-128.1084f, -144.5226f, -258.0896f),
        new Vector3(-128.4761f, -144.5166f, -262.4524f),
    };

    private DateTime laughingLeapEnds = DateTime.MinValue;

    private DateTime fodderEnds = DateTime.MinValue;
    private Vector3 fodderTetherPoint = Vector3.Zero;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.DohnMheg;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.DohnMheg;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        13547, 13952,
    };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        // Boss 2: Toad Choir
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheThroneRoom,
            objectSelector: (bc) => bc.CastingSpellId == ToadChoir,
            leashPointProducer: () => LordOfLengthsomeGaitArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 135.0f);

        // Boss 3: Imp Choir gaze attack
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheThroneRoom,
            objectSelector: bc => bc.CastingSpellId == ImpChoir && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TeagGye,
            () => LordOfLingeringGazeArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheAtelier,
            () => GriauleArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 22.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheThroneRoom,
            () => LordOfLengthsomeGaitArenaCenter,
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
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.TeagGye:
                result = await HandleLordOfLingeringGazeAsync();
                break;
            case SubZoneId.TheAtelier:
                result = await HandleGriauleAsync();
                break;
            case SubZoneId.TheThroneRoom:
                result = await HandleLordOfLengthsomeGaitAsync();
                break;
        }

        return result;
    }

    private async Task<bool> HandleLordOfLingeringGazeAsync()
    {
        BattleCharacter boss1 = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(LordOfLingeringGaze)
            .FirstOrDefault(bc => bc.IsTargetable && bc.Distance() < 50);

        if (laughingLeap.IsCasting() && laughingLeapEnds < DateTime.Now)
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, LaughingLeapDuration, $"Dodging Laughing Leap / Landsblood geysers");
            laughingLeapEnds = DateTime.Now.AddMilliseconds(LaughingLeapDuration);
        }

        if (DateTime.Now < laughingLeapEnds)
        {
            // Intentionally follow tank to find melee uptime since no cleave damage is occurring
            await MovementHelpers.GetClosestTank.FollowTimed(laughingLeapSw, LaughingLeapDuration);
        }

        return false;
    }

    private async Task<bool> HandleGriauleAsync()
    {
        BattleCharacter boss2 = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Griaule)
            .FirstOrDefault(bc => bc.IsTargetable && bc.Distance() < 50);

        if (boss2 != null)
        {
            // Saplings change target in real-time to whomever's currently blocking their tether.
            // Since there are always 5 saplings + 3 Trust members, wait for them to pick their tethers,
            // then try to block the closest remaining tether.
            IEnumerable<BattleCharacter> unhandledSaplings = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(PaintedSapling)
                .Where(bc => bc.CurrentTargetId == boss2.ObjectId);

            if (unhandledSaplings.Count() == 2 && fodderEnds < DateTime.Now)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, FodderDuration, $"Blocking sapling tether");
                fodderEnds = DateTime.Now.AddMilliseconds(FodderDuration);

                BattleCharacter sapling = unhandledSaplings.OrderBy(bc => bc.Distance()).First();

                Vector3 start = boss2.Location;
                Vector3 end = sapling.Location;
                fodderTetherPoint = start.GetPointBetween(end, 8.0f);

                Logger.Information($"Blocking sapling tether: {start} <- {start.Distance(fodderTetherPoint):N2} -> {fodderTetherPoint} <- {fodderTetherPoint.Distance(end):N2} -> {end}");
            }

            if (DateTime.Now < fodderEnds && Core.Player.Distance(fodderTetherPoint) > FodderDistance)
            {
                MoveToParameters parameters = new MoveToParameters
                {
                    Location = fodderTetherPoint,
                    DistanceTolerance = FodderDistance,
                };

                await CommonTasks.MoveAndStop(parameters, FodderDistance);
            }
        }

        return false;
    }

    private async Task<bool> HandleLordOfLengthsomeGaitAsync()
    {
        BattleCharacter liarsLyre = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(LiarsLyre)
            .FirstOrDefault(bc => bc.CastingSpellId == Finale);

        if (liarsLyre != null && liarsLyre.Location.Distance2D(Core.Player.Location) >= 10.0f)
        {
            if (Core.Player.IsCasting)
            {
                ActionManager.StopCasting();
            }

            Navigator.PlayerMover.MoveStop();

            SpellCastInfo finale = liarsLyre.SpellCastInfo;
            TimeSpan finaleDuration = finale.RemainingCastTime;
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, finaleDuration, $"Walking tight-rope for ({finale.ActionId}) {finale.Name} for up to {finaleDuration.TotalMilliseconds:N0}ms");

            foreach (Vector3 point in tightRopeWalkPoints)
            {
                Logger.Information($"Next tight-rope point: {point}");

                while (point.Distance2D(Core.Player.Location) > 0.2f)
                {
                    Navigator.PlayerMover.MoveTowards(point);
                    await Coroutine.Sleep(30);
                }
            }

            Navigator.PlayerMover.MoveStop();
            CapabilityManager.Clear(CapabilityHandle, reason: $"Finished walking tight-rope for ({finale.ActionId}) {finale.Name} with {finaleDuration.TotalMilliseconds:N0}ms remaining");
        }

        return false;
    }
}
