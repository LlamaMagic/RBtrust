using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
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
/// Lv. 50.3: The Porta Decumana dungeon logic.
/// </summary>
public class PortaDecumana : AbstractDungeon
{
    private const uint TheUltimaWeaponNpc = 2137;
    private const int AetheroplasmNpc = 2138;

    private static readonly HashSet<uint> RadiantBlaze = new() {28991};

    private static readonly HashSet<uint> HomingRay = new() {29011, 29012};
    private static readonly int HomingRayDuration = 5_000;

    private const uint GeocrushSpell = 28999;
    private const uint EyeoftheStormSpell = 28980;
    private const uint VulcanBurstSpell = 29003;
    private const uint LaserFocusSpell = 29014;
    private const uint CitadelBusterSpell = 29020;
    private const uint ExplosionSpell = 29021;

    DateTime EyeoftheStormDuration = DateTime.Now.AddMilliseconds(30);

    private static readonly HashSet<uint> CitadelBuster = new() {29020};
    private static readonly HashSet<uint> LaserFocus = new() {29013, 29014};
    private static readonly int CitadelBusterDuration = 5_000;
    private static readonly int LaserFocusDuration = 5_000;
    private static DateTime citadelBusterTimestamp = DateTime.MinValue;

    private static readonly Vector3 UltimaArenaCenter1 = new(-771.9428f, -400.0628f, -600.3899f);
    private static readonly Vector3 UltimaArenaCenter2 = new(-703.6115f, -185.6595f, 479.6159f);

    /*


(2137) The Ultima Weapon, Dist: 2.79f, Loc: <-713, -185.7316, 480>, IsTargetable: False, Target: None
  └─ Casting (29012) Homing Ray => (0) Lv. 50 Reaper
(2137) The Ultima Weapon, Dist: 2.80f, Loc: <-713.0083, -185.7245, 479.9724>, IsTargetable: True, Target: Storm Marauder
  └─ Casting (29011) Homing Ray => (2137) The Ultima Weapon
  avoid other players

     */

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.ThePortaDecumana;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ThePortaDecumana;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Ultima Titan: GeoCrush
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            objectSelector: bc => bc.CastingSpellId == GeocrushSpell,
            radiusProducer: bc => 25.0f,
            priority: AvoidancePriority.High));

        // The Ultima Weapon: Eye of the Storm
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            objectSelector: c => c.CastingSpellId == EyeoftheStormSpell,
            outerRadius: 90.0f,
            innerRadius: 12f,
            priority: AvoidancePriority.Medium);

        // Ultima Ifrit: Vulcan Burst
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            objectSelector: c => c.CastingSpellId == VulcanBurstSpell,
            outerRadius: 40.0f,
            innerRadius: 3.0F,
            priority: AvoidancePriority.Medium);

        // Boss 1
        // Let's avoid standing under the boss if we can help it.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            objectSelector: bc => bc.NpcId == TheUltimaWeaponNpc && bc.CanAttack,
            radiusProducer: bc => 4.3f,
            priority: AvoidancePriority.Medium));

        // The Ultima Weapon: Citadel Buster
        // Every time I tried to use a Rectangle here I couldn't get it to face in front of the mob. Always went off the side.
        // So i took the easy way out and made it a cone
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            objectSelector: (bc) => bc.CastingSpellId == CitadelBusterSpell,
            leashPointProducer: () => UltimaArenaCenter2,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 180.0f);

        // The Ultima Weapon: Explosion
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            objectSelector: bc => bc.CastingSpellId == ExplosionSpell,
            radiusProducer: bc => 17.4f,
            priority: AvoidancePriority.High));

        /* The Ultima Weapon: Laser Focus
         Need to make a way to set the donut around the party member that's being targetted, not the spell caster
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            objectSelector: c => c.CastingSpellId == LaserFocusSpell,
            outerRadius: 40.0f,
            innerRadius: 3.0F,
            priority: AvoidancePriority.Medium);
            */

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            () => UltimaArenaCenter1,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ThePortaDecumana,
            () => UltimaArenaCenter2,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (Core.Me.IsHealer())
        {
            BattleCharacter tankPlayer = PartyManager.AllMembers
                .Select(pm => pm.BattleCharacter)
                .OrderBy(obj => obj.Distance(Core.Player))
                .FirstOrDefault(obj => !obj.IsMe && obj.IsTank() && obj.IsValid);

            if (Core.Me.Location.Distance2D(tankPlayer.Location) > 25)
            {
                await CommonTasks.MoveTo(tankPlayer.Location);
                await Coroutine.Sleep(30);
            }
        }

        if (LaserFocus.IsCasting())
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, LaserFocusDuration,
                $"Stacking for Laser Focus");

            BattleCharacter laserFocusTarget = PartyManager.AllMembers
                .Select(pm => pm.BattleCharacter)
                .OrderBy(obj => obj.Distance(Core.Player))
                .FirstOrDefault(obj => !obj.IsMe);

            if (Core.Me.Location.Distance2D(laserFocusTarget.Location) > 5)
            {
                await CommonTasks.MoveTo(laserFocusTarget.Location);
                await Coroutine.Sleep(30);
                await CommonTasks.StopMoving();
            }

        }

        if (HomingRay.IsCasting())
        {
            await MovementHelpers.Spread(HomingRayDuration);
        }

        await Coroutine.Yield();

        return false;
    }
}
