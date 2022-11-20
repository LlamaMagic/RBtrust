using Buddy.Coroutines;
using ff14bot.Managers;
using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 80.3: Anamnesis Anyder dungeon logic.
/// </summary>
public class AnamnesisAnyder : AbstractDungeon
{
    // SpellName: Scrutiny SpellId: 20005 CurrentHealth:54.36572 Follow
    // SpellName: Inscrutability SpellId: 19306 CurrentHealth:34.33089 room wide aoe
    // SpellName: Clearout SpellId: 19307 CurrentHealth:18.67048 cone aoe
    // SpellName: Luminous Ray SpellId: 20007 CurrentHealth:85.04648 straight lazer
    // SpellName: Ectoplasmic Ray SpellId: 19322 CurrentHealth:48.1767 STACK
    // SpellName: Eye of the Cyclone SpellId: 19287 CurrentHealth:88.68323 DONUT
    // SpellName: 2000-mina swing SpellId: 19285 CurrentHealth:74.37302 Follow
    // SpellName: 2000-mina swipe SpellId: 19284 CurrentHealth:64.34352 Cone
    // SpellName: the Final Verse SpellId: 19288 CurrentHealth:98.03452 room wide aoe
    // SpellName: Raging Glower SpellId: 19286,28266
    // SpellName: Terrible Hammer SpellId: 19289 CurrentHealth:87.74754
    // SpellName: Terrible Blade SpellId: 19290 CurrentHealth:85.54825
    // SpellName: Wanderer's Pyre 19291, 19295 spread
    // SpellName: Open Hearth 19292, 19296 stack
    // SpellName: Depth Grip SpellId: 19332 CurrentHealth:90.52967 stack
    // SpellName: Falling Water SpellId: 19325 CurrentHealth:88.45962 spread
    // SpellName: Rising Tide SpellId: 19339 CurrentHealth:86.6134 stack
    // SpellName: Flying Fount stack 19327, 19328
    private const uint UnknownA = 9260;
    private const uint UnknownB = 9261;
    private const uint Kyklops = 9263;

    private static readonly HashSet<uint> NonSidestepSpells = new()
    {
        19289,
        19290,
        19286,
        19291,
        19295,
        19325,
        19327,
        19328,
        19332,
        19339,
        19292,
        19296,
        19285,
        19287,
        19322,
        20005,
        20006,
        20007,
        28266,
    };

    private static readonly HashSet<uint> LuminousRayA = new() { 20007 };
    private static readonly HashSet<uint> LuminousRayB = new() { 20007 };
    private static readonly int LuminousRayDuration = 6_000;

    private static readonly HashSet<uint> RagingGlower = new() { 19286, 28266 };
    private static readonly int RagingGlowerDuration = 6_000;

    private static readonly HashSet<uint> WanderersPyre = new() { 19291, 19295 };
    private static readonly int WanderersPyreDuration = 6_000;

    private static readonly HashSet<uint> FallingWater = new() { 19325 };
    private static readonly int FallingWaterDuration = 6_000;

    private static DateTime ragingGlowerTimestamp = DateTime.MinValue;
    private static DateTime luminousRayTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AnamnesisAnyder;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AnamnesisAnyder;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        19293,
        19289,
        19294,
        19290,
        19294,
        19293,
        19327,
        19328,
        19332,
        19339,
        19292,
        19296,
        19285,
        19287,
        19322,
        20005,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (LuminousRayA.IsCasting() && luminousRayTimestamp < DateTime.Now)
        {
            BattleCharacter unknownNpcA = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(UnknownA)
                .FirstOrDefault(bc => bc.IsTargetable);

            SidestepPlugin.Enabled = true;
            AvoidanceHelpers.AddAvoidRectangle(unknownNpcA, 12.0f, 40.0f);
            luminousRayTimestamp = DateTime.Now.AddMilliseconds(LuminousRayDuration);
        }

        if (LuminousRayB.IsCasting() && luminousRayTimestamp < DateTime.Now)
        {
            BattleCharacter unknownNpcB = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(UnknownB)
                .FirstOrDefault(bc => bc.IsTargetable);

            SidestepPlugin.Enabled = true;
            AvoidanceHelpers.AddAvoidRectangle(unknownNpcB, 12.0f, 40.0f);
            luminousRayTimestamp = DateTime.Now.AddMilliseconds(LuminousRayDuration);
        }

        if (RagingGlower.IsCasting() && ragingGlowerTimestamp < DateTime.Now)
        {
            BattleCharacter kyklopsNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Kyklops)
                .FirstOrDefault(bc => bc.IsTargetable);

            SidestepPlugin.Enabled = true;
            AvoidanceHelpers.AddAvoidRectangle(kyklopsNpc, 12.0f, 40.0f);
            ragingGlowerTimestamp = DateTime.Now.AddMilliseconds(RagingGlowerDuration);
        }

        if (WanderersPyre.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(WanderersPyreDuration);
        }

        if (FallingWater.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(FallingWaterDuration, 10f);
        }

        SidestepPlugin.Enabled = !NonSidestepSpells.IsCasting();

        await Coroutine.Yield();

        return false;
    }
}
