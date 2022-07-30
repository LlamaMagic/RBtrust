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
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 85: Vanaspati dungeon logic.
/// </summary>
public class Vanaspati : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.Vanaspati;

    private readonly HashSet<uint> follow = new()
    {
        25139, 25140,
    };

    private readonly HashSet<uint> follow1 = new()
    {
        25151, 25153,
    };

    private readonly HashSet<uint> follow2 = new()
    {
        25160, 25166,
    };

    private readonly HashSet<uint> magnet = new()
    {
        25143, 25146, 25148, 27852,
    };

    private readonly HashSet<uint> magnet1 = new()
    {
        25145,
    };

    private readonly HashSet<uint> magnet2 = new()
    {
        25147,
    };

    private readonly HashSet<uint> magnet3 = new()
    {
       25142, 25168,
    };

    private readonly HashSet<uint> magnet4 = new()
    {
        25169,
    };

    private readonly HashSet<string> overStr = new()
    {
       "地脉失控", "污秽的",
    };

    private readonly AvoidInfo avoidNull = AvoidanceManager.AddAvoidLocation(() => false, 0, () => new Vector3("0,0,1"));

    private readonly Stopwatch followSW = new();
    private readonly Stopwatch follow1SW = new();
    private readonly Stopwatch follow2SW = new();
    private readonly Stopwatch followxSW = new();

    private readonly Stopwatch magnetSW = new();
    private readonly Stopwatch magnet1SW = new();
    private readonly Stopwatch magnet2SW = new();
    private readonly Stopwatch magnet3SW = new();
    private readonly Stopwatch magnet3fW = new();

    private readonly Stopwatch magnet3xSW = new();
    private readonly Stopwatch magnet3xsSW = new();
    private readonly Stopwatch magnet4SW = new();
    private readonly Stopwatch magnet4xSW = new();
    private readonly Stopwatch magnetxSW = new();

    private readonly Stopwatch hastargetSW = new();
    private readonly Stopwatch hastargetxSW = new();

    private DateTime resetTime = DateTime.Now;

    private bool pmbuff;
    private bool magnet3SWhaifrun;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Vanaspati;

    private static bool HasTarget => GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false).
                Any(bc => bc.CanAttack && bc.IsTargetable);

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        if (WorldManager.SubZoneId != 4012 && WorldManager.SubZoneId != 4013 && WorldManager.SubZoneId != 4014)
        {
            BattleCharacter target = GameObjectManager.Attackers?.OrderByDescending(e => e.CurrentHealthPercent).FirstOrDefault();

            if (target != null && Core.Player.CurrentTarget != target)
            {
                if (DateTime.Now > resetTime)
                {
                    resetTime = DateTime.Now.AddSeconds(3);

                    target.Target();
                }

                if (!SidestepPlugin.Enabled)
                {
                    SidestepPlugin.Enabled = true;
                }
            }

            if (magnet3SWhaifrun)
            {
                magnet3SWhaifrun = false;
            }

            magnet3.Remove(25160);
            magnet3.Remove(25169);
            hastargetxSW.Reset();
            hastargetSW.Reset();
            magnet3SW.Reset();
        }

        if (Core.Player.HasAura("Temporary Misdirection") || pmbuff)
        {
            if (!pmbuff)
            {
                pmbuff = true;
            }

            SidestepPlugin.Enabled = false;

            if (!Core.Player.HasAura("Temporary Misdirection") && pmbuff)
            {
                SidestepPlugin.Enabled = true;
                CapabilityManager.Clear();
                pmbuff = false;
            }
        }

        if (WorldManager.SubZoneId == 4014 && !Core.Player.InCombat)
        {
            if (magnet3SWhaifrun)
            {
                magnet3SWhaifrun = false;
            }

            magnet3.Remove(25160);
            magnet3.Remove(25169);
            hastargetxSW.Reset();
            hastargetSW.Reset();
            magnet3SW.Reset();
        }

        if (WorldManager.SubZoneId == 4014 && Core.Player.InCombat)
        {
            if (!HasTarget && !hastargetxSW.IsRunning)
            {
                hastargetxSW.Restart();
            }

            if (!HasTarget && !hastargetSW.IsRunning)
            {
                followxSW.Reset();
                follow2SW.Reset();
                Navigator.PlayerMover.MoveStop();
            }

            if (HasTarget && hastargetxSW.ElapsedMilliseconds < 3_000 && hastargetxSW.IsRunning)
            {
                hastargetxSW.Reset();
            }

            if ((!HasTarget && hastargetxSW.ElapsedMilliseconds > 3_000) || hastargetSW.IsRunning)
            {
                if (!hastargetSW.IsRunning)
                {
                    Logger.Information($"自动躲闪开启 hastarget {HasTarget}");
                    SidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => true);
                    AvoidanceManager.AddAvoid(avoidNull);
                    AvoidanceManager.Pulse();
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 30_000, "自动跟随");
                    hastargetSW.Restart();
                    followxSW.Reset();
                    follow2SW.Reset();
                }

                if (hastargetSW.ElapsedMilliseconds > 15_000)
                {
                    ActionManager.Sprint();
                }

                if (HasTarget)
                {
                    hastargetxSW.Reset();

                    Logger.Information($"自动躲闪开启 hastarget {HasTarget}");
                    SidestepPlugin.Enabled = true;
                    CapabilityManager.Clear();
                    hastargetSW.Reset();
                    magnet3.Add(25169);
                    magnet3SWhaifrun = true;
                }
                else
                {
                    await MovementHelpers.GetClosestAlly.Follow();
                }
            }
        }

        if (follow.IsCasting() && !magnet3SW.IsRunning || followSW.IsRunning)
        {
            if (!followSW.IsRunning)
            {
                Logger.Information($"自动跟随队友 followSW {follow.IsCasting()}");
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => true);
                AvoidanceManager.AddAvoid(avoidNull);
                AvoidanceManager.Pulse();

                CapabilityManager.Clear();
                followSW.Restart();

                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 15_000, "自动跟随");
            }

            if (!follow.IsCasting())
            {
                Logger.Information($"自动躲闪开启 followSW {followSW.ElapsedMilliseconds}");
                SidestepPlugin.Enabled = true;
                CapabilityManager.Clear();
                followSW.Reset();
            }
            else
            {
                await MovementHelpers.GetClosestAlly.Follow();
            }
        }

        if (follow1.IsCasting() || follow1SW.IsRunning)
        {
            if (!follow1SW.IsRunning)
            {
                Logger.Information($"自动跟随队友 follow1SW {follow1.IsCasting()}");
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => true);
                AvoidanceManager.AddAvoid(avoidNull);
                AvoidanceManager.Pulse();

                CapabilityManager.Clear();
                follow1SW.Restart();

                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 15_000, "自动跟随");
            }

            if (!follow1.IsCasting())
            {
                Logger.Information($"自动躲闪开启 follow1SW {follow1SW.ElapsedMilliseconds}");
                SidestepPlugin.Enabled = true;
                CapabilityManager.Clear();
                follow1SW.Reset();
            }
            else
            {
                if (follow1SW.ElapsedMilliseconds < 2_000)
                {
                    Vector3 location = new(-294.9383f, 41.5f, -354.0579f);

                    if (Core.Me.Distance2D(location) > 1f)
                    {
                        Navigator.PlayerMover.MoveTowards(location);
                    }
                    else
                    {
                        MovementManager.MoveStop();
                    }
                }
                else
                {
                    await MovementHelpers.GetClosestPartyMember(new Vector3(-294.9383f, 41.5f, -354.0579f)).Follow(2f);
                }
            }
        }

        if (followxSW.ElapsedMilliseconds > 3_000)
        {
            Logger.Information($"自动躲闪开启 followxSW {followxSW.ElapsedMilliseconds}");
            SidestepPlugin.Enabled = true;
            CapabilityManager.Clear();
            followxSW.Reset();
        }

        if (!hastargetSW.IsRunning && !magnet3SW.IsRunning && !magnet3xsSW.IsRunning && (follow2.IsCasting() || follow2SW.IsRunning))
        {
            if (!follow2SW.IsRunning)
            {
                Logger.Information($"自动跟随队友 follow2SW {follow2.IsCasting()}");
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => true);
                AvoidanceManager.AddAvoid(avoidNull);
                AvoidanceManager.Pulse();

                CapabilityManager.Clear();
                follow2SW.Restart();
                followxSW.Reset();
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 15_000, "自动跟随");
            }

            if (!follow2.IsCasting())
            {
                Logger.Information($"followxSW 开启");
                if (!followxSW.IsRunning)
                {
                    followxSW.Restart();
                    follow2SW.Reset();
                }
            }
            else
            {
                await MovementHelpers.GetClosestPartyMember(new Vector3(300.0752f, 55.00583f, -156.6629f)).Follow();
            }
        }

        if (magnet.IsCasting() || magnetSW.IsRunning)
        {
            if (!magnetSW.IsRunning)
            {
                if (WorldManager.SubZoneId == 4013)
                {
                    SidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => true);
                    AvoidanceManager.AddAvoid(avoidNull);
                    AvoidanceManager.Pulse();

                    CapabilityManager.Clear();
                }

                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "正在移动");

                magnetSW.Restart();
            }

            if (!magnet.IsCasting())
            {
                CapabilityManager.Clear();

                if (!SidestepPlugin.Enabled)
                {
                    SidestepPlugin.Enabled = true;
                }

                magnetSW.Reset();
            }
            else
            {
                if (WorldManager.SubZoneId == 4012)
                {
                    Vector3 location = new(-375.0211f, 14.5f, 84.97742f);

                    if (Core.Me.Distance2D(location) > 1f)
                    {
                        Navigator.PlayerMover.MoveTowards(location);
                    }
                    else
                    {
                        MovementManager.MoveStop();
                    }
                }

                if (WorldManager.SubZoneId == 4013)
                {
                    Vector3 location = new(-294.9383f, 41.5f, -354.0579f);

                    if (Core.Me.Distance2D(location) > 1f)
                    {
                        Navigator.PlayerMover.MoveTowards(location);
                    }
                    else
                    {
                        MovementManager.MoveStop();
                    }
                }
            }
        }

        if (magnet1.IsCasting() || magnet1SW.IsRunning)
        {
            if (!magnet1SW.IsRunning)
            {
                if (WorldManager.SubZoneId == 4013)
                {
                    SidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => true);
                    AvoidanceManager.AddAvoid(avoidNull);
                    AvoidanceManager.Pulse();

                    CapabilityManager.Clear();

                    magnetxSW.Restart();
                }

                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 3_000, "正在移动");

                magnet1SW.Restart();
            }

            await MovementHelpers.Spread(3_000, 8.0f);
        }

        if (magnet2.IsCasting() || magnet2SW.IsRunning)
        {
            if (!magnet2SW.IsRunning)
            {
                if (WorldManager.SubZoneId == 4013)
                {
                    SidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => true);
                    AvoidanceManager.AddAvoid(avoidNull);
                    AvoidanceManager.Pulse();

                    CapabilityManager.Clear();
                    magnet1SW.Reset();
                    magnetxSW.Reset();
                }

                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "正在移动");

                magnet2SW.Restart();
            }

            if (!magnet2.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => true);
                AvoidanceManager.AddAvoid(avoidNull);
                AvoidanceManager.Pulse();

                CapabilityManager.Clear();

                SidestepPlugin.Enabled = true;

                magnet2SW.Reset();
            }
            else
            {
                if (WorldManager.SubZoneId == 4013)
                {
                    Vector3 usets = (Vector3)GameObjectManager.GetObjectsOfType<BattleCharacter>()?.Where(obj => obj.NpcId == 1383).OrderBy(obj => Core.Player.Distance2D(obj)).FirstOrDefault().Location;

                    if (Core.Me.Distance2D(usets) > 0.5f)
                    {
                        Navigator.PlayerMover.MoveTowards(usets);
                    }
                    else
                    {
                        MovementManager.MoveStop();
                    }
                }
            }
        }

        ReceiveMessageHelpers.SkillsdeterminationOverStr = overStr;

        if (magnet3.IsCasting() || magnet3SW.IsRunning)
        {
            if (!magnet3SW.IsRunning)
            {
                followSW.Reset();
                followxSW.Reset();
                follow2SW.Reset();
                magnet3fW.Reset();
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => true);
                AvoidanceManager.AddAvoid(avoidNull);
                AvoidanceManager.Pulse();

                CapabilityManager.Clear();
                magnet3.Add(25160);

                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 15_000, "正在移动");

                magnet3SW.Restart();

                ReceiveMessageHelpers.SkillsdeterminationOverStatus = false;
            }

            if (magnet3SWhaifrun && WorldManager.SubZoneId == 4014)
            {
                if (magnet3SW.ElapsedMilliseconds < 7_500 && !ReceiveMessageHelpers.SkillsdeterminationOverStatus)
                {
                    await MovementHelpers.GetClosestPartyMember(new Vector3(300.0752f, 55.00583f, -156.6629f)).Follow();
                }
                else
                {
                    if (!magnet3fW.IsRunning)
                    {
                        magnet3fW.Start();
                    }

                    if (magnet3fW.ElapsedMilliseconds < 600)
                    {
                        if (MovementHelpers.GetClosestAlly.Distance(new Vector3(300.0752f, 55.00583f, -156.6629f)) - 3f < Core.Player.Distance(new Vector3(300.0752f, 55.00583f, -156.6629f)))
                        {
                            Navigator.PlayerMover.MoveTowards(new Vector3(300.0752f, 55.00583f, -156.6629f));
                        }
                        else
                        {
                            Navigator.PlayerMover.MoveStop();
                        }
                    }
                    else
                    {
                        if (!AvoidanceManager.IsRunningOutOfAvoid)
                        {
                            await MovementHelpers.SpreadSpLoc(3_000, new Vector3(300.0752f, 55.00583f, -156.6629f), 6.5f);
                        }
                    }
                }
            }
            else
            {
                if (WorldManager.SubZoneId == 4014)
                {
                    if (magnet3SW.ElapsedMilliseconds < 4_000)
                    {
                        await MovementHelpers.GetClosestPartyMember(new Vector3(299.9771f, 55.00583f, -157.0001f)).Follow();
                    }
                    else
                    {
                        if (!AvoidanceManager.IsRunningOutOfAvoid)
                        {
                            await MovementHelpers.SpreadSp(3_000, new Vector3(299.9771f, 55.00583f, -157.0001f), 6f);
                        }
                    }
                }

                if (WorldManager.SubZoneId == 4012)
                {
                    if (magnet3SW.ElapsedMilliseconds < 2_000 && !ReceiveMessageHelpers.SkillsdeterminationOverStatus)
                    {
                        await MovementHelpers.GetClosestAlly.Follow();
                    }
                    else
                    {
                        if (!AvoidanceManager.IsRunningOutOfAvoid)
                        {
                            await MovementHelpers.HalfSpread(3_000, 7f, 10717);
                        }
                    }
                }
            }

            if (!magnet3.IsCasting())
            {
                magnet3SW.Reset();
                magnet3fW.Reset();
                CapabilityManager.Clear();
                AvoidanceManager.RemoveAllAvoids(i => true);
                AvoidanceManager.AddAvoid(avoidNull);
                AvoidanceManager.Pulse();
                SidestepPlugin.Enabled = true;
                magnet3.Remove(25160);
                Logger.Information($"magnet3SWrun 运行结束");
                ReceiveMessageHelpers.SkillsdeterminationOverStatus = false;
            }
        }

        return false;
    }
}
