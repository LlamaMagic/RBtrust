using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 83: The Tower of Babil dungeon logic.
/// </summary>
public class TheTowerOfBabil : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.TheTowerOfBabil;

    // SPELLIDS

    // BARNABAS (B1)
    // Ground and Pound    25159
    // Ground and Pound    25322
    // Dynamic Pound       25326
    // Dynamic Pound       25157
    // Shocking Force (St) 25324
    // Dynamic Scrapline   25158
    // Dynamic Scrapline   25328
    // Thundercall         25325
    // Rolling Scrapline   25323

    // LUGAE (B2)
    // Thermal Suppression 25338
    // Magitek Missile     25334
    // Magitek Ray         25340
    // Magitek Chakram     25331
    // Magitek Explosive   25336
    // Downpour            25333

    // ANIMA (B3)
    // Lunar Nail         25342
    // Phantom Pain       21182
    // Mega Graviton      25344
    // Pater Patriae      25350
    // Boundless Pain     25347
    // Imperatum          25353
    // Obliviating Claw   25355
    // Obliviating Claw 2 25354
    // Erupting Pain      25351
    private readonly HashSet<uint> follow = new()
    {
        21182, 25324,
    };

    private readonly HashSet<uint> magnet = new()
    {
        25326, 25157, 25158, 25328,
    };

    private readonly HashSet<uint> toad = new()
    {
        25333,
    };

    private readonly HashSet<uint> mini = new()
    {
        25331,
    };

    private readonly HashSet<uint> boundlessPain = new()
    {
        25347,
    };

    private readonly HashSet<uint> spread = new()
    {
        25351,
    };

    private readonly HashSet<uint> claw2 = new()
    {
        25354,
    };

    private readonly Stopwatch followTimer = new();
    private readonly Stopwatch magnetTimer = new();
    private readonly Stopwatch miniTimer = new();
    private readonly Stopwatch toadTimer = new();
    private readonly Stopwatch boundlessPainTimer = new();
    private readonly Stopwatch claw2Timer = new();
    private readonly Stopwatch spreadTimer = new();

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheTowerOfBabil;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        if (follow.IsCasting() || followTimer.IsRunning)
        {
            if (!followTimer.IsRunning)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                CapabilityManager.Clear();
                followTimer.Restart();
            }

            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 2_500, "Follow/Stack Mechanic In Progress");
            await MovementHelpers.GetClosestAlly.Follow();

            if (!follow.IsCasting())
            {
                SidestepPlugin.Enabled = true;
                followTimer.Reset();
            }
        }

        if (magnet.IsCasting() || magnetTimer.IsRunning)
        {
            if (!magnetTimer.IsRunning)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                CapabilityManager.Clear();
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 12_000, "Magnet Spell In Progress");
                magnetTimer.Restart();
            }

            if (magnetTimer.ElapsedMilliseconds < 12_000)
            {
                Vector3 location = new(-314.4527f, -175f, 70.98297f);

                if (Core.Me.Distance(location) > 1f)
                {
                    Navigator.PlayerMover.MoveTowards(location);
                }
                else
                {
                    MovementManager.MoveStop();
                }
            }

            if (magnetTimer.ElapsedMilliseconds >= 12_000)
            {
                SidestepPlugin.Enabled = true;
                magnetTimer.Reset();
            }
        }

        if (toad.IsCasting() || toadTimer.IsRunning)
        {
            if (!toadTimer.IsRunning)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                CapabilityManager.Clear();
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 30_000, "Shapeshift Mechanic In Progress");
                toadTimer.Restart();
            }

            if (toadTimer.ElapsedMilliseconds < 12_000)
            {
                Vector3 location = new(214.2467f, 0.9999993f, 306.0189f);

                if (Core.Me.Distance(location) < 1f)
                {
                    MovementManager.MoveStop();
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(location);
                }
            }

            if (toadTimer.ElapsedMilliseconds >= 12_000 && toadTimer.ElapsedMilliseconds < 30_000)
            {
                await MovementHelpers.GetClosestAlly.Follow();
            }

            if (toadTimer.ElapsedMilliseconds >= 30_000)
            {
                SidestepPlugin.Enabled = true;
                toadTimer.Reset();
            }
        }

        if (mini.IsCasting() || miniTimer.IsRunning)
        {
            if (!miniTimer.IsRunning)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                CapabilityManager.Clear();
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 24_000, "Shapeshift Mechanic In Progress");
                miniTimer.Restart();
            }

            if (miniTimer.ElapsedMilliseconds < 12_000)
            {
                Vector3 location = new(227.0484f, 1.00001f, 305.9774f);

                if (Core.Me.Distance(location) < 1f)
                {
                    MovementManager.MoveStop();
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(location);
                }
            }

            if (miniTimer.ElapsedMilliseconds >= 12_000 && miniTimer.ElapsedMilliseconds < 24_000)
            {
                Vector3 location = new(220.9772f, 1f, 305.9483f);

                if (Core.Me.Distance(location) < 1f)
                {
                    MovementManager.MoveStop();
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(location);
                }
            }

            if (miniTimer.ElapsedMilliseconds >= 24_000)
            {
                SidestepPlugin.Enabled = true;
                miniTimer.Reset();
            }
        }

        if (claw2.IsCasting() || claw2Timer.IsRunning)
        {
            if (!claw2Timer.IsRunning)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                CapabilityManager.Clear();
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 12_000, "Obliviating Claw 2 In Progress");
                claw2Timer.Restart();
            }

            if (claw2Timer.ElapsedMilliseconds < 6_000)
            {
                Vector3 location = new(16.74083f, 120f, -406.9069f);

                if (Core.Me.Distance(location) < 1f)
                {
                    MovementManager.MoveStop();
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(location);
                }
            }

            if (claw2Timer.ElapsedMilliseconds >= 6_000 && claw2Timer.ElapsedMilliseconds < 12_000)
            {
                Vector3 location = new(-15.15774f, 120f, -408.2812f);

                if (Core.Me.Distance(location) < 1f)
                {
                    MovementManager.MoveStop();
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(location);
                }
            }

            if (claw2Timer.ElapsedMilliseconds >= 12_000)
            {
                SidestepPlugin.Enabled = true;
                claw2Timer.Reset();
            }
        }

        if (boundlessPain.IsCasting() || boundlessPainTimer.IsRunning)
        {
            if (!boundlessPainTimer.IsRunning)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                CapabilityManager.Clear();
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 18_000, "Boundless Pain Avoid");
                boundlessPainTimer.Restart();
            }

            if (boundlessPainTimer.ElapsedMilliseconds >= 8_000 && boundlessPainTimer.ElapsedMilliseconds < 18_000)
            {
                Vector3 location = new(11.11008f, 479.9997f, -199.1336f);

                if (Core.Me.Distance(location) < 1f)
                {
                    Navigator.Stop();
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(location);
                }
            }

            if (boundlessPainTimer.ElapsedMilliseconds >= 18_000)
            {
                SidestepPlugin.Enabled = true;
                boundlessPainTimer.Reset();
            }
        }

        if (spread.IsCasting() || spreadTimer.IsRunning)
        {
            if (!spreadTimer.IsRunning)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Spread");
                spreadTimer.Start();
            }

            if (spreadTimer.ElapsedMilliseconds < 5_000)
            {
                await MovementHelpers.Spread(5_000);
            }

            if (spreadTimer.ElapsedMilliseconds >= 5_000)
            {
                spreadTimer.Reset();
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            }
        }

        // Avoid Claw Game in final zone only if in combat, otherwise frequent stucks
        if (WorldManager.SubZoneId == 4133)
        {
            SidestepPlugin.Enabled = Core.Me.InCombat;
        }

        return false;
    }
}
