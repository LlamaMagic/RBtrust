using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 50.5: The Keeper of the Lake dungeon logic.
/// </summary>
public class KeeperOfTheLake : AbstractDungeon
{
    /*
     * 1. Einhander NPCID: 3369 SubzoneID: 1503
     * 2. Magitek Gunship NPCID: 3373 SubzoneID: 1505
     * 3. Midgardsormr NPCID: 3374 SubzoneID: 1507
     */

    /* Einhander
     * SpellName: Mark XLIII Mini Cannon SpellId: 29272 follow
     * SpellName: Heavy Swing SpellId: 29620 tank buster
     */

    /* Magitek Gunship
     *
     *
     */

    /* Midgardsormr
     * SpellName: Phantom Inner Turmoil SpellId: 29278
     * SpellName: Phantom Outer Turmoil SpellId: 29279  Follow
     * SpellName: Akh Morn SpellId: 29283 Stack
     * SpellName: Antipathy SpellId: 29285 follow
     */

    private const int EinhanderNPCID = 3369;
    private const int MagitekGunshipNPCID = 3373;
    private const int MidgardsormrNPCID = 3374;

    private const uint FlameThrowerSpell = 3389;
    private const uint PhantomOuterTurmoilSpell = 29279;

    private static readonly Vector3 EinhanderArenaCenter = new(18.7437f, 26.65833f, -16.99149f);
    private static readonly Vector3 MagitekGunshipArenaCenter = new(8.534668f, 346.0237f, -149.6482f);
    private static readonly Vector3 MidgardsormrArenaCenter = new(-40.8009f, 641.0406f, -78.09348f);

    private static readonly Vector3 MidgardsormrLeashPoint = new(-40.7861f, 640.1833f, -98.2496f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheKeeperOfTheLake;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheKeeperOfTheLake;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { 29283 };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2: Magitek Gunship Garlean Fire
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<GameObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CeruleumSpill,
            objectSelector: bc => bc.NpcId == 2005194 && bc.IsVisible,
            radiusProducer: bc => 8.5f,
            priority: AvoidancePriority.High));

        // Boss 2: Magitek Gunship Flame Thrower
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CeruleumSpill,
            objectSelector: (bc) => bc.CastingSpellId == FlameThrowerSpell,
            leashPointProducer: () => MagitekGunshipArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 125.0f);

        // Boss 2
        // In general, if not tank stay out of the front to avoid Garlean Fire
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CeruleumSpill &&
                          !Core.Me.IsTank(),
            objectSelector: (bc) =>
                bc.NpcId == MagitekGunshipNPCID &&
                bc.CanAttack, // Had to use CanAttack here, as there's invisble NPCs all around the room
            leashPointProducer: () => MagitekGunshipArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 40.0f);

        // Boss 3: Toric Phantom Outer Turmoil
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheForswornPromise,
            objectSelector: c => c.CastingSpellId == PhantomOuterTurmoilSpell,
            outerRadius: 90f,
            innerRadius: 19f,
            priority: AvoidancePriority.Medium);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AgriusHull,
            () => EinhanderArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CeruleumSpill,
            () => MagitekGunshipArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheForswornPromise,
            () => MidgardsormrArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 18.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
