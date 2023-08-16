using Buddy.Coroutines;
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
/// Lv. 79: Mt. Gulg dungeon logic.
/// </summary>
public class MtGulg : AbstractDungeon
{
    private const int ForgivenCruelty = 8260;
    private const int ForgivenWhimsy = 8261;
    private const int Brightsphere = 7864;
    private const int JudgmentDayTower = 2009807;
    private const int ForgivenRevelry = 8270;
    private const int ForgivenObscenity = 8262;
    private const int ForgivenDissonance = 8299;

    /// <summary>
    /// Set of boss-related monster IDs.
    /// </summary>
    private static readonly HashSet<uint> BossIds = new()
    {
        Brightsphere,
        ForgivenCruelty,
        ForgivenWhimsy,
        ForgivenObscenity,
        ForgivenRevelry,
        ForgivenDissonance,
    };

    private static readonly HashSet<uint> LumenInfinitum = new() { 16818 };
    private static readonly HashSet<uint> Exegesis = new() { 15622, 15623, 16987, 16988, 16989, };
    private static readonly HashSet<uint> RightPalm = new() { 16247, 16248 };
    private static readonly HashSet<uint> LeftPalm = new() { 16249, 16250 };
    private static readonly HashSet<uint> GoldChaser = new() { 15652, 15653, 17066 };

    private static readonly HashSet<uint> PenancePianissimo = new() { 15644 };
    private static readonly Vector3 PenancePianissimoCenter = new(-239.9347f, 210.0f, 236.9791f);
    private static readonly int PenancePianissimoDuration = 45_000;
    private static DateTime penancePianissimoTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.MtGulg;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.MtGulg;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        15614, 15615, 15616, 15617, 15618, 15622, 15623, 15638, 15640, 15641, 15642, 15643, 15644, 15645, 15648, 15649,
        16247, 16248, 16249, 16250, 16521, 16818, 16987, 16988, 16989, 17153, 18025,
    };

    /// <inheritdoc/>
    public override async Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ThePerishedPath,
            () => ArenaCenter.ForgivenCruelty,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWhiteGate,
            () => ArenaCenter.ForgivenWhimsy,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheWindingFlare,
            innerWidth: 38.0f,
            innerHeight: 38.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.ForgivenObscenity },
            priority: AvoidancePriority.High);


        return false;
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (!Core.Player.InCombat)
        {
            AvoidanceManager.RemoveAllAvoids(ai => !ai.CanRun);
        }

        if (WorldManager.SubZoneId == (uint)SubZoneId.ThePerishedPath)
        {
            if (LumenInfinitum.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }
        }

        if (WorldManager.SubZoneId == (uint)SubZoneId.TheWhiteGate)
        {
            if (Exegesis.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }
        }

        if (WorldManager.SubZoneId == (uint)SubZoneId.TheFalsePrayer)
        {
            BattleCharacter forgivenRevelryNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenRevelry)
                    .FirstOrDefault(bc => bc.Distance() < 50);
            if (forgivenRevelryNpc != null && forgivenRevelryNpc.IsValid)
            {
                if (RightPalm.IsCasting() || LeftPalm.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }
            }
        }

        if (WorldManager.SubZoneId == (uint)SubZoneId.TheWindingFlare)
        {
            BattleCharacter forgivenObscenityNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenObscenity)
                    .FirstOrDefault(bc => bc.Distance() < 50);
            if (forgivenObscenityNpc != null && forgivenObscenityNpc.IsValid)
            {
                if (PenancePianissimo.IsCasting() && penancePianissimoTimestamp < DateTime.Now)
                {
                    penancePianissimoTimestamp = DateTime.Now.AddMilliseconds(PenancePianissimoDuration);

                    // Create an AOE avoid for the orange swirly under the boss
                    AvoidanceHelpers.AddAvoidDonut(
                        canRun: () => Core.Player.InCombat && DateTime.Now < penancePianissimoTimestamp,
                        locationProducer: () => PenancePianissimoCenter,
                        outerRadius: 30,
                        innerRadius: 15);
                }

                if (GoldChaser.IsCasting())
                {
                    Stopwatch sw = new();
                    sw.Start();
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 20_000, "Forgiven Obscenity Avoid");
                    while (sw.ElapsedMilliseconds < 20_000)
                    {
                        await MovementHelpers.GetClosestAlly.Follow();
                        await Coroutine.Yield();
                    }

                    sw.Stop();
                }
            }
        }

        if (WorldManager.SubZoneId != (uint)SubZoneId.TheWindingFlare)
        {
            BossIds.ToggleSideStep(new uint[] { ForgivenObscenity });
        }
        else
        {
            BossIds.ToggleSideStep();
        }

        return false;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Forgiven Cruelty.
        /// </summary>
        public static readonly Vector3 ForgivenCruelty = new(188f, -48f, -170f);

        /// <summary>
        /// Second Boss: Forgiven Whimsy.
        /// </summary>
        public static readonly Vector3 ForgivenWhimsy = new(-240f, 210f, -49.5f);

        /// <summary>
        /// Third Boss: Forgiven Obscenity.
        /// </summary>
        public static readonly Vector3 ForgivenObscenity = new(-240f, 210f, 237f);
    }
}
