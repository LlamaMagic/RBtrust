using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
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
/// Lv. 71: Holminster Switch dungeon logic.
/// </summary>
public class HolminsterSwitch : AbstractDungeon
{
    private const int ForgivenDissonance = 8299;
    private const int TesleenTheForgiven = 8300;
    private const int Philia = 8301;
    private const int IronChainObj = 8570;

    private const int BleedingAura = 320;

    /// <summary>
    /// Set of boss-related monster IDs.
    /// </summary>
    private static readonly HashSet<uint> BossIds = new()
    {
        ForgivenDissonance,
        TesleenTheForgiven,
        Philia,
        IronChainObj,
    };

    private static readonly HashSet<uint> Thumbscrew = new() { 15814, 16850 };
    private static readonly HashSet<uint> BrazenBull = new() { 15817, 15820 };
    private static readonly HashSet<uint> Exorcise = new() { 15826, 15827 };
    private static readonly HashSet<uint> FeveredFlagellation = new() { 15829, 15830, 17440 };
    private static readonly HashSet<uint> RightKnout = new() { 15846 };
    private static readonly HashSet<uint> LeftKnout = new() { 15847 };
    private static readonly HashSet<uint> IntotheLight = new() { 15847, 17232 };
    private static readonly HashSet<uint> Taphephobia = new() { 15842, 16769 };
    private static readonly HashSet<uint> FierceBeating = new() { 15834, 15835, 15836, 15837, 15838, 15839, };

    private static readonly Vector3 ExorciseStackLoc = new(79.35034f, 0f, -81.01664f);
    private static readonly int ExorciseDuration = 25_000;

    private static readonly HashSet<uint> Pendulum = new() { 15833, 15842, 16769, 16777, 16790, };
    private static readonly Vector3 PendulumDodgeLoc = new(117.1188f, 23f, -474.0881f);

    private static readonly int FierceBeatingDuration = 32_000;
    private static DateTime fierceBeatingTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.HolminsterSwitch;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.HolminsterSwitch;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        15602, 15609, 15814, 15815, 15816, 15817, 15818, 15819, 15820, 15822, 15843, 15845, 15846, 15847, 15848, 15849,
        15886, 16765, 16779, 16780, 16781, 16782, 16850, 16851, 16852, 17232, 17552,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        BattleCharacter forgivenDissonanceNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenDissonance)
            .FirstOrDefault(bc => bc.IsTargetable);

        if (forgivenDissonanceNpc != null && forgivenDissonanceNpc.IsValid)
        {
            if (Thumbscrew.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }

            if (BrazenBull.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }
        }

        BattleCharacter tesleenNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: TesleenTheForgiven)
            .FirstOrDefault(bc => bc.IsTargetable);

        if (tesleenNpc != null && tesleenNpc.IsValid)
        {
            if (Exorcise.IsCasting())
            {
                if (Core.Me.Distance(ExorciseStackLoc) > 1f && Core.Me.IsCasting)
                {
                    ActionManager.StopCasting();
                }

                while (Core.Me.Distance(ExorciseStackLoc) > 1f)
                {
                    await CommonTasks.MoveTo(ExorciseStackLoc);
                    await Coroutine.Yield();
                }

                // Wait in-place for stack marker to go off
                Navigator.PlayerMover.MoveStop();
                await Coroutine.Sleep(5_000);

                Stopwatch exorciseTimer = new();
                exorciseTimer.Restart();

                // Create an AOE avoid for the ice puddle where the stack marker went off
                AvoidanceManager.AddAvoidLocation(
                    () => exorciseTimer.IsRunning && exorciseTimer.ElapsedMilliseconds < ExorciseDuration,
                    radius: 6.5f * 1.5f, // Expand to account for stack target maybe standing to the side
                    () => ExorciseStackLoc);

                // Non-targetable but technically .IsVisible copies of Tesleen with the same .NpcId are used to place the outer ice puddles
                // Create AOE avoids on top of them since SideStep doesn't do this automatically
                IEnumerable<GameObject> fakeTesleens =
                    GameObjectManager.GetObjectsByNPCId(TesleenTheForgiven).Where(obj => !obj.IsTargetable);
                foreach (GameObject fake in fakeTesleens)
                {
                    Vector3 location = fake.Location;

                    ff14bot.Pathing.Avoidance.AvoidInfo a = AvoidanceManager.AddAvoidLocation(
                        () => exorciseTimer.IsRunning && exorciseTimer.ElapsedMilliseconds < ExorciseDuration,
                        radius: 6.5f,
                        () => location);
                }
            }

            if (FeveredFlagellation.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.Spread(10_000, 10);
            }

            if (Core.Me.HasAura(BleedingAura))
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }
        }

        BattleCharacter philiaNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: Philia)
            .FirstOrDefault(bc => bc.IsTargetable);

        if (philiaNpc != null && philiaNpc.IsValid)
        {
            if (RightKnout.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }

            if (LeftKnout.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }

            if (IntotheLight.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }

            if (Taphephobia.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.Spread(10_000, 9.0f);
            }

            if (Pendulum.IsCasting())
            {
                if (Core.Me.Distance(PendulumDodgeLoc) > 1 && Core.Me.IsCasting)
                {
                    ActionManager.StopCasting();
                }

                while (Core.Me.Distance(PendulumDodgeLoc) > 1.0f)
                {
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 3_000, "Pendulum Avoid");
                    await CommonTasks.MoveTo(PendulumDodgeLoc);
                    await Coroutine.Yield();
                }

                await CommonTasks.StopMoving();
                await Coroutine.Sleep(100);
            }

            if (FierceBeating.IsCasting() &&
                fierceBeatingTimestamp.AddMilliseconds(FierceBeatingDuration) < DateTime.Now)
            {
                if (philiaNpc != null)
                {
                    Vector3 location = philiaNpc.Location;
                    uint objectId = philiaNpc.ObjectId;

                    fierceBeatingTimestamp = DateTime.Now;
                    Stopwatch fierceBeatingTimer = new();
                    fierceBeatingTimer.Restart();

                    // Create an AOE avoid for the orange swirly under the boss
                    AvoidanceManager.AddAvoidObject<GameObject>(
                        canRun: () =>
                            fierceBeatingTimer.IsRunning &&
                            fierceBeatingTimer.ElapsedMilliseconds < FierceBeatingDuration,
                        radius: 11f,
                        unitIds: objectId);

                    // Attach very wide cone avoid pointing out the boss's right, forcing bot to left side
                    // Boss spins clockwise and front cleave comes quickly, so disallow less-safe right side
                    // Position + rotation will auto-update as the boss moves + turns!
                    AvoidanceManager.AddAvoidUnitCone<GameObject>(
                        canRun: () =>
                            fierceBeatingTimer.IsRunning &&
                            fierceBeatingTimer.ElapsedMilliseconds < FierceBeatingDuration,
                        objectSelector: (obj) => obj.ObjectId == objectId,
                        leashPointProducer: () => location,
                        leashRadius: 40f,
                        rotationDegrees: -90f,
                        radius: 25f,
                        arcDegrees: 345f);
                }
            }
        }

        BossIds.ToggleSideStep();

        return false;
    }
}
