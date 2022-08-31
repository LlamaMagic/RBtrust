using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
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
    private const uint EvilDreamerNpc = 11382;
    private const uint UniteMareSpell = 29622;
    private const int VoidGravityDuration = 6_250;

    private const uint BeatriceNpc = 11384;
    private const uint BeatificScornSpell = 29817;
    private const int BeatificScornDuration = 10_000;
    private const int VoidNailDuration = 5_250;
    private const uint DeathForeseenSingleSpell = 29821;
    private const uint DeathForeseenMultiSpell = 29828;
    private const uint VoidshakerSpell = 29822;

    private const uint ScarmiglioneNpc = 11372;
    private const uint WallSegmentNpc = 108;
    private const uint BlightedBedevilmentSpell = 30235;
    private const uint VacuumWaveSpell = 30236;
    private const int RottenRampageDuration = 15_250;
    private const int FiredampDuration = 5_750;
    private const int BlightedBedevilmentDuration = 5_750;

    private static readonly HashSet<uint> VoidGravity = new() { 29626, 30022, 30242, 30023 };
    private static readonly HashSet<uint> VoidNail = new() { 29823 };
    private static readonly HashSet<uint> RottenRampage = new() { 30231 };
    private static readonly HashSet<uint> VoidVortex = new() { 30024, 30025, 30243, 30253, 30254, };
    private static readonly HashSet<uint> ToricVoid = new() { 29829, 31207, 31206, };

    private static readonly HashSet<uint> Firedamp = new() { 30262, 30263 };

    private static readonly Vector3 EvilDreamersArenaCenter = new(168f, -700f, 90f);
    private static readonly Vector3 BeatriceArenaCenter = new(0f, -698f, -148f);
    private static readonly Vector3 ScarmiglioneArenaCenter = new(-35.5f, 385f, -298.5f);

    private AvoidInfo blightedBedevilmentAvoid = default;
    private DateTime blightedBedevilmentEnds = DateTime.MinValue;

    /* Boss List
     * 1. Evil Dreamer NPC Id: 11382 Subzone Id: 4184
     * 2. Beatrice NPC Id: 11384 Subzone Id: 4185
     * 3. Scarmiglione NPC Id: 11372 Subzone Id: 4186
     */

    /* Evil Dreamer Spells
     * Void Gravity - Spread to avoid 29626,30022,30242,30023
     * Unite Mare - Need to follow NPCs
     *   - 29621: May be the one of three heads that grows large suddenly
     *   - 29622, 29628: Can probably avoid via SideStep
     * Endless Nightmare - Kill boss before it goes off - SpellId: 29630
     */

    /* Beatrice Spells
     * SpellName: Eye of Troia SpellId: 29818 - Turn away about 10 seconds after cast starts
     * SpellName: Death Foreseen SpellId: 29821, 29828 - Actual gaze cast for Eye of Troia. 29828 multi-point version
     * SpellName: Hush SpellId: 29824 Tank buster
     * SpellName: Beatific Scorn SpellId: 29813 - NPC follow for about 30 seconds after cast starts
     * SpellName: Void Nail SpellId: 29823 - Spread
     * SpellName: Toric Void SpellId: 29829, 31207, 31206 - Donut
     * SpellName: Antipressure SpellId: 31208 - Stack
     * SpellName: Voidshaker SpellId: 29822 - Cone
     */

    /* Scarmiglione Spells
     * SpellName: Cursed Echo SpellId: 30257 - Room-wide AOE, nothing to do
     * SpellName: Rotten Rampage SpellId: 30028,30031,30056,30231,30232,30233 spread
     *   - Looks like 30231 does the actual spread
     * SpellName: Void Vortex SpellId: 30024,30025,30243,30253,30254 stack
     * SpellName: Blighted Bedevilment SpellId: 30235 CurrentHealth:83.15511 sidestep
     * SpellName: Blighted Bladework SpellId: 30259 CurrentHealth:75.88284 sidestep
     * SpellName: Blighted Sweep SpellId: 30261 CurrentHealth:75.26254 sidestep
     * SpellName: Firedamp SpellId: 30262 CurrentHealth:6.20486 AOE Tank buster
     */

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheFellCourtOfTroia;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheFellCourtOfTroia;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { 30024, 30025, 30243, 30253, 30254, 31208 };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Unite-Mare's growing AOE
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Penitence,
            objectSelector: bc => bc.CastingSpellId == UniteMareSpell,
            radiusProducer: bc => 20.0f,
            priority: AvoidancePriority.High));

        // Boss 2: Beatific Scorn with progressive avoid priority
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == BeatificScornSpell && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds < BeatificScornDuration * 0.33,
            radiusProducer: bc => 11.0f,
            priority: AvoidancePriority.High));

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == BeatificScornSpell && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds >= BeatificScornDuration * 0.33,
            radiusProducer: bc => 11.0f,
            priority: AvoidancePriority.Medium));

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == BeatificScornSpell && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds >= BeatificScornDuration * 0.66,
            radiusProducer: bc => 11.0f,
            priority: AvoidancePriority.Low));

        // Boss 2: Voidshaker cone
        // TODO: Remove this, Toric Void, and SideStep toggle in XML after SideStep updates with type-10 donut support
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: (bc) => bc.CastingSpellId == VoidshakerSpell,
            leashPointProducer: () => BeatriceArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 125.0f);

        // Boss 2: Toric Void donut
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost
                && GameObjectManager.GetObjectsOfType<Character>().Any(c => ToricVoid.Contains(c.CastingSpellId)),
            () => BeatriceArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 11.0f,
            priority: AvoidancePriority.High);

        // Boss 2: Eye of Troia / Death Foreseen single-gaze attack
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == DeathForeseenSingleSpell && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.High));

        // Boss 2: Eye of Troia / Death Foreseen multi-gaze attack
        // Conveniently, the multi-point gaze is a separate spell ID from the single version.
        // Since the boss is centered, we can add them large circle-avoids to stand in the "butt-crack"
        // between avoids and implicitly look away due to attacking/facing the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            objectSelector: bc => bc.CastingSpellId == DeathForeseenMultiSpell,
            radiusProducer: bc => 16.0f,
            priority: AvoidancePriority.Medium));

        // Boss 3: Blighted Bedevilment inner circle
        // Vacuum Wave knockback handled more dynamically in RunAsync()
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GardenOfEpopts,
            objectSelector: bc => bc.CastingSpellId == BlightedBedevilmentSpell,
            radiusProducer: bc => 12.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Penitence,
            () => EvilDreamersArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SeatOfTheForemost,
            () => BeatriceArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GardenOfEpopts,
            () => ScarmiglioneArenaCenter,
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
            case SubZoneId.Penitence:
                result = await HandleEvilDreamersAsync();
                break;
            case SubZoneId.SeatOfTheForemost:
                result = await HandleBeatriceAsync();
                break;
            case SubZoneId.GardenOfEpopts:
                result = await HandleScarmiglioneAsync();
                break;
        }

        return result;
    }

    private async Task<bool> HandleEvilDreamersAsync()
    {
        if (VoidGravity.IsCasting())
        {
            await MovementHelpers.Spread(VoidGravityDuration);
        }

        return false;
    }

    private async Task<bool> HandleBeatriceAsync()
    {
        if (VoidNail.IsCasting())
        {
            await MovementHelpers.Spread(VoidNailDuration);
        }

        return false;
    }

    private async Task<bool> HandleScarmiglioneAsync()
    {
        if (RottenRampage.IsCasting())
        {
            await MovementHelpers.Spread(RottenRampageDuration);
        }

        if (Firedamp.IsCasting())
        {
            await MovementHelpers.Spread(FiredampDuration);
        }

        if (VoidGravity.IsCasting())
        {
            await MovementHelpers.Spread(VoidGravityDuration);
        }

        BattleCharacter boss3 = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(ScarmiglioneNpc)
            .FirstOrDefault(bc => bc.IsTargetable && bc.CastingSpellId == BlightedBedevilmentSpell);

        if (boss3 != default && blightedBedevilmentEnds < DateTime.Now)
        {
            blightedBedevilmentEnds = DateTime.Now.AddMilliseconds(BlightedBedevilmentDuration);

            // Drawing cone avoids doesn't support dynamic rotation via a "rotation producer" Func<float>,
            // so we can manually add/remove AvoidInfos to update the rotation argument.
            AvoidanceManager.RemoveAvoid(blightedBedevilmentAvoid);

            // The mechanic involves a knockback to a 15 degree-wide destructible wall section,
            // so we can draw a 360-15 = 345 degree "cone" and point it directly away from the nearest wall
            // to create an avoid-compatible safe space that plays nice with other simultaneous avoids.
            // Adding 15 degrees found experimentally because the wall's "real" position is off-center.
            Vector3 wallSegmentLocation = GameObjectManager.GetObjectsByNPCId(WallSegmentNpc)
                .OrderBy(obj => obj.Distance())
                .First().Location;

            float rotation = 15f + MathEx.CalculateNeededFacing(wallSegmentLocation, ScarmiglioneArenaCenter);

            // Add updated cone and save it for later removal
            blightedBedevilmentAvoid = AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
                canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GardenOfEpopts,
                objectSelector: (bc) => bc.CastingSpellId == VacuumWaveSpell,
                leashPointProducer: () => ScarmiglioneArenaCenter,
                leashRadius: 40.0f,
                rotationDegrees: rotation,
                radius: 40.0f,
                arcDegrees: 345.0f);
        }

        return false;
    }
}
