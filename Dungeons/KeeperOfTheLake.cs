using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
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

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheKeeperOfTheLake;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheKeeperOfTheLake;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        29272,
        29278,
        29279,
        29283,
        29285,
    };

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

        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
