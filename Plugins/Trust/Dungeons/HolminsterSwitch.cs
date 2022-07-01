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

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 71: Holminster Switch dungeon logic.
    /// </summary>
    public class HolminsterSwitch : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.HolminsterSwitch;

        private const int ForgivenDissonance = 8299;
        private const int TesleenTheForgiven = 8300;
        private const int Philia = 8301;
        private const int IronChainObj = 8570;

        /// <summary>
        /// Set of boss-related monster IDs.
        /// </summary>
        private static readonly HashSet<uint> BossIds = new HashSet<uint>
        {
            ForgivenDissonance,
            TesleenTheForgiven,
            Philia,
            IronChainObj,
        };

        /// <summary>
        /// Set of spells to dodge by following closest ally.
        /// </summary>
        private static readonly HashSet<uint> SpellsDodgedViaClosest = new HashSet<uint>()
        {
            // 15602, 15609                             :: Heretic's Fork
            // 15814, 16850                             :: Thumbscrew
            // 15815, 16852                             :: Wooden Horse
            // 15816, 16851                             :: Gibbet Cage
            // 15817, 15820                             :: Brazen Bull
            // 15818                                    :: Executioner's Sword
            // 15819                                    :: Light Shot
            // 15822, 15886, 17552                      :: Heretic's Fork
            // 15834, 15835, 15836, 15837, 15838, 15839 :: Fierce Beating
            // 15840, 15841                             :: Cat o' Nine Tails
            // 15843, 16765                             :: Sickly Inferno
            // 15845, 17232                             :: Into the Light
            // 15846                                    :: Right Knout
            // 15847                                    :: Left Knout
            // 15848, 15849                             :: Aethersup
            // 16779, 16780, 16781, 16782               :: Land Rune
            15602,
            15609,
            15814,
            15815,
            15816,
            15817,
            15818,
            15819,
            15820,
            15822,
            15843,
            15845,
            15846,
            15847,
            15848,
            15849,
            15886,
            16765,
            16779,
            16780,
            16781,
            16782,
            16850,
            16851,
            16852,
            17232,
            17552,
        };

        private static readonly HashSet<uint> Thumbscrew = new HashSet<uint>() { 15814, 16850 };
        private static readonly HashSet<uint> BrazenBull = new HashSet<uint>() { 15817, 15820 };
        private static readonly HashSet<uint> Exorcise = new HashSet<uint>() { 15826, 15827 };
        private static readonly HashSet<uint> FeveredFlagellation = new HashSet<uint>() { 15829, 15830, 17440 };
        private static readonly HashSet<uint> RightKnout = new HashSet<uint>() { 15846 };
        private static readonly HashSet<uint> LeftKnout = new HashSet<uint>() { 15847 };
        private static readonly HashSet<uint> IntotheLight = new HashSet<uint>() { 15847, 17232 };
        private static readonly HashSet<uint> Taphephobia = new HashSet<uint>() { 15842, 16769 };
        private static readonly HashSet<uint> FierceBeating = new HashSet<uint>() { 15834, 15835, 15836, 15837, 15838, 15839, };

        private static readonly Vector3 ExorciseStackLoc = new Vector3("79.35034, 0, -81.01664");
        private static readonly int ExorciseDuration = 25_000;

        private static readonly HashSet<uint> Pendulum = new HashSet<uint>() { 15833, 15842, 16769, 16777, 16790, };
        private static readonly Vector3 PendulumDodgeLoc = new Vector3("117.1188,23,-474.0881");

        private static readonly int FierceBeatingDuration = 32_000;
        private static DateTime fierceBeatingTimestamp = DateTime.MinValue;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.HolminsterSwitch;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            BattleCharacter forgivenDissonanceNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenDissonance)
                .FirstOrDefault(bc => bc.Distance() < 50);

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
                .FirstOrDefault(bc => bc.Distance() < 50);

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

                    Stopwatch exorciseTimer = new Stopwatch();
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
                    await MovementHelpers.Spread(10000, 10);
                }

                if (Core.Me.HasAura(320))
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }
            }

            BattleCharacter philiaNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: Philia)
                .FirstOrDefault(bc => bc.Distance() < 50);

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
                        CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 3000, "Pendulum Avoid");
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
                        Stopwatch fierceBeatingTimer = new Stopwatch();
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

            if (SpellsDodgedViaClosest.IsCasting())
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 1500, "Spells Avoid");
                await MovementHelpers.GetClosestAlly.Follow();
            }

            BossIds.ToggleSideStep();

            return false;
        }
    }
}
