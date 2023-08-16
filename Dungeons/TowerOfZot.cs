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
/// Lv. 81: The Tower of Zot dungeon logic.
/// </summary>
public class TowerOfZot : AbstractDungeon
{
    private readonly CapabilityManagerHandle trustHandle3 = CapabilityManager.CreateNewHandle();
    private readonly CapabilityManagerHandle trustHandle4 = CapabilityManager.CreateNewHandle();

    private readonly Stopwatch daSw = new();
    private readonly Stopwatch tmSw = new();
    private readonly Stopwatch stSw = new();

    // BOSS MECHANIC SPELLIDS

    // B1 - Minduruva
    // Manusya Bio        25248
    // Manusya Blizzard   25234
    // Manusya Fire III   25233
    // Manusya Thunder    25235
    // Manusya Bio III    25236
    // Transmute Fire III 25242
    // Manusya Fire       25699
    // Dhrupad            25244
    // Transmute Thunder  25372
    // Transmute Blizzard 25371
    // Transmute Bio III  25373

    // B2 - Sanduruva
    // Isitva Siddhi   25257
    // Prapti Siddhi   25256
    // Manusya Berserk 25249
    // Explosive Force 25250
    // Prakamya Siddhi 25251
    // Manusya Stop    25255
    // Manusya Confuse 25253

    // B3 (3 sisters at once)
    // Cinduruva
    // Samsara          25273
    // Manusya Reflect  25259

    // Sanduruva
    // Manusya Faith    25258
    // Isitva Siddhi    25280
    // Prapti Siddhi    25275

    // Minduruva
    // Dhrupad          25281
    // Manusya Fire     25287
    // Manusya Blizzard 25288
    // Manusya Thunder  25289
    // Delta Attack     25260
    // Delta Attack     25261
    // Delta Attack     25262

    // TRASH WITH NO OMEN
    // Zot Roader
    // Haywire 24145

    // B1
    private readonly HashSet<uint> transmute = new()
    {
        25242, 25372, 25371, 25373,
    };

    private readonly HashSet<uint> deltaattack = new() { 25260, 25261, 25262, };

    private const int Sanduruva = 10257;

    // Prakamya Siddhi
    private readonly HashSet<uint> prakamyaSiddhi = new() { 25251 };

    private static DateTime prakamyaSiddhiTimestamp = DateTime.MinValue;

    private const int PrakamyaSiddhiDuration = 6_000;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheTowerOfZot;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheTowerOfZot;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        25234, 25233, 25250, 24145,
    };

    /// <inheritdoc/>
    public override async Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.IngenuitysIngress,
            () => ArenaCenter.Minduruva,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ProsperitysPromise,
            () => ArenaCenter.Sanduruva,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WisdomsWard,
            () => ArenaCenter.Cinduruva,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);


        return false;
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (!Core.Me.InCombat)
        {
            CapabilityManager.Clear();
            daSw.Reset();
            tmSw.Reset();
            stSw.Reset();
        }

        if (stSw.ElapsedMilliseconds > 3_000)
        {
            stSw.Reset();
            SidestepPlugin.Enabled = true;
        }

        if (deltaattack.IsCasting() || (daSw.IsRunning && daSw.ElapsedMilliseconds < 24_000))
        {
            if (!daSw.IsRunning || daSw.ElapsedMilliseconds >= 24_000)
            {
                CapabilityManager.Update(trustHandle3, CapabilityFlags.Movement, 24_000, "Delta Attack Avoid");
                CapabilityManager.Update(trustHandle4, CapabilityFlags.Facing, 24_000, "Delta Attack Avoid");
                daSw.Restart();
            }

            if (daSw.ElapsedMilliseconds < 24_000)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.FollowTimed(daSw, 24_000);
            }
        }

        if (daSw.ElapsedMilliseconds > 24_000)
        {
            SidestepPlugin.Enabled = true;
        }

        BattleCharacter sanduruvaNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: Sanduruva)
            .FirstOrDefault(bc => bc.Distance() < 50 && bc.IsTargetable);
        if (sanduruvaNpc != null && sanduruvaNpc.IsValid)
        {
            if (prakamyaSiddhi.IsCasting() &&
                prakamyaSiddhiTimestamp.AddMilliseconds(PrakamyaSiddhiDuration) < DateTime.Now)
            {
                Vector3 location = sanduruvaNpc.Location;
                uint objectId = sanduruvaNpc.ObjectId;

                prakamyaSiddhiTimestamp = DateTime.Now;
                Stopwatch prakamyaSiddhiTimer = new();
                prakamyaSiddhiTimer.Restart();

                // Create an AOE avoid for the Prakamya Siddhi around the boss
                AvoidanceManager.AddAvoidObject<GameObject>(
                    canRun: () =>
                        prakamyaSiddhiTimer.IsRunning &&
                        prakamyaSiddhiTimer.ElapsedMilliseconds < PrakamyaSiddhiDuration,
                    radius: 5f,
                    unitIds: objectId);
            }
        }

        if (transmute.IsCasting() || tmSw.IsRunning)
        {
            if (!tmSw.IsRunning)
            {
                CapabilityManager.Update(trustHandle3, CapabilityFlags.Movement, 20_000, "Transmute Avoid");
                CapabilityManager.Update(trustHandle4, CapabilityFlags.Facing, 20_000, "Transmute Avoid");
                tmSw.Restart();
            }

            if (tmSw.ElapsedMilliseconds < 20_000)
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.FollowTimed(tmSw, 20_000);
            }

            if (tmSw.ElapsedMilliseconds >= 20_000)
            {
                SidestepPlugin.Enabled = true;
                tmSw.Reset();
            }
        }

        return false;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Minduruva.
        /// </summary>
        public static readonly Vector3 Minduruva = new(68f, -443f, -124.5f);

        /// <summary>
        /// Second Boss: Sanduruva.
        /// </summary>
        public static readonly Vector3 Sanduruva = new(-258f, -169f, -26f);

        /// <summary>
        /// Third Boss: Cinduruva.
        /// </summary>
        public static readonly Vector3 Cinduruva = new(-23f, 546f, -54f);
    }
}
