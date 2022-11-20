using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 55: The Aery dungeon logic.
/// </summary>
public class Aery : AbstractDungeon
{
    /*
    * 1. Rangda NPCID: 3452 SubzoneID: 1577
    * 2. Gyascutus NPCID: 3455 SubzoneID: 1580
    * 3. Nidhogg NPCID: 3458 SubzoneID: 1582
    */

    /* Rangda
     * SpellName: Electric Cachexia SpellId: 3889 Follow
     * SpellName: Ionospheric Charge SpellId: 3888
     * SpellName: Electrocution SpellId: 3890
     * Lightning Rod 2574
     */

    /* Gyascutus
     * SpellName: Proximity Pyre SpellId: 30191 SideStep
     * SpellName: Proximity Pyre SpellId: 30191
     * SpellName: Ashen Ouroboros SpellId: 30190 Donut
     * SpellName: Body Slam SpellId: 31234 Follow
     * SpellName: Crippling Blow SpellId: 30193 Tank Buster
     * SpellName: Inflammable Fumes SpellId: 30181
     */

    /* Nidhogg
     * SpellName: the Sable Price SpellId: 30203
     * SpellName: Horrid Blaze SpellId: 30224
     * SpellName: Hot Tail SpellId: 30196
     * SpellName: the Scarlet Price SpellId: 30205 tank buster
     * Hot Wing 30195
     */

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAery;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAery;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        3889,
        30181,
        30190,
        30195,
        30224,
        31234,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
