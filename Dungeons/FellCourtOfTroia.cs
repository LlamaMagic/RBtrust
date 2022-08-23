using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
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
/// Lv. 90.3: The Fell Court of Troia dungeon logic.
/// </summary>
public class FellCourtOfTroia : AbstractDungeon
{
    private const uint Beatrice = 11384;

    private static readonly HashSet<uint> VoidGravity = new() {29626, 30022, 30242, 30023};
    private static readonly HashSet<uint> VoidNail = new() {29823};
    private static readonly HashSet<uint> BeatificScorn = new() {29813};
    private static readonly HashSet<uint> EyeofTroia = new() {29818};
    private static readonly HashSet<uint> RottenRampage = new() {30231};
    private static readonly HashSet<uint> VoidVortex = new()
    {
        30024,
        30025,
        30243,
        30253,
        30254,
    };
    private static readonly HashSet<uint> Firedamp = new() {30262};

    private static readonly int VoidGravityDuration = 10_000;
    private static readonly int BeatificScornDuration = 30_000;
    private static readonly int VoidNailDuration = 10_000;
    private static readonly int RottenRampageDuration = 10_000;
    private static readonly int FiredampDuration = 10_000;
    private static DateTime beatificScornTimestamp = DateTime.MinValue;

    private DateTime beatificScornEnds = DateTime.MinValue;

    private readonly Stopwatch beatificScornSw = new();

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheFellCourtOfTroia;

    /* Boss List
     * 1. Evil Dreamer NPC Id: 11382 Subzone Id: 4184
     * 2. Beatrice NPC Id: 11384 Subzone Id: 4185
     * 3. Scarmiglione NPC Id: 11372 Subzone Id: 4186
     */

    /* Evil Dreamer Spells
     * Void Gravity - Spread to avoid 29626,30022,30242,30023
     * Unite Mare - Need to follow NPCs 29621,29622,29628
     * Endless Nightmare - Kill boss before it goes off - SpellId: 29630
     */

    /* Beatrice Spells
     * SpellName: Eye of Troia SpellId: 29818 - Turn away about 10 seconds after cast starts
     * SpellName: Hush SpellId: 29824 Tank buster
     * SpellName: Beatific Scorn SpellId: 29813 - NPC follow for about 30 secnds after cast starts
     * SpellName: Void Nail SpellId: 29823 - Spread
     */


    /* Scarmiglione Spells
     * SpellName: Cursed Echo SpellId: 30257 room wide aoe, nothing to do
     * SpellName: Rotten Rampage SpellId: 30028,30031,30056,30231,30232,30233 spread
     *  Looks like 30231 does the actual spread
     * SpellName: Void Vortex SpellId: 30024,30025,30243,30253,30254 stack
     * SpellName: Blighted Bedevilment SpellId: 30235 CurrentHealth:83.15511 sidestep
     * SpellName: Blighted Bladework SpellId: 30259 CurrentHealth:75.88284 sidestep
     *  SpellName: Blighted Sweep SpellId: 30261 CurrentHealth:75.26254 sidestep
     * SpellName: Firedamp SpellId: 30262 CurrentHealth:6.20486 AOE Tank buster
     */

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheFellCourtOfTroia;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        29621,
        29622,
        29628,
        30024,
        30025,
        30243,
        30253,
        30254,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (VoidGravity.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(VoidGravityDuration);
        }

        if (BeatificScorn.IsCasting())
        {
            BattleCharacter beatriceNPC = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Beatrice)
                .FirstOrDefault(bc => bc.IsTargetable && bc.Distance() < 50);

            if (BeatificScorn.IsCasting() && beatificScornEnds < DateTime.Now)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, BeatificScornDuration,
                    $"Dodging Beatific Scorn");
                beatificScornEnds = DateTime.Now.AddMilliseconds(BeatificScornDuration);
            }

            if (DateTime.Now < beatificScornEnds)
            {
                await MovementHelpers.GetClosestDps.FollowTimed(beatificScornSw, BeatificScornDuration);
            }
        }


        if (EyeofTroia.IsCasting())
        {
            BattleCharacter beatriceNPC = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Beatrice)
                .FirstOrDefault(bc => bc.IsTargetable && bc.Distance() < 50);

            SpellCastInfo eyeofTroia = beatriceNPC.SpellCastInfo;
            TimeSpan gazeDuration = eyeofTroia.RemainingCastTime + TimeSpan.FromMilliseconds(8000);

            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, gazeDuration,
                $"Looking away from ({eyeofTroia.ActionId}) {eyeofTroia.Name} for {gazeDuration.TotalMilliseconds:N0}ms");
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Facing, gazeDuration);
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Targeting, gazeDuration);

            ActionManager.StopCasting();
            Core.Player.ClearTarget();
            Core.Player.FaceAway(beatriceNPC);
            await Coroutine.Sleep(gazeDuration);
        }

        if (VoidNail.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(VoidNailDuration);
        }

        if (RottenRampage.IsCasting())
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(RottenRampageDuration);
        }

        if (Firedamp.IsCasting() && !Core.Me.IsTank())
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(FiredampDuration);
        }

        SidestepPlugin.Enabled = true;
        return false;
    }
}
