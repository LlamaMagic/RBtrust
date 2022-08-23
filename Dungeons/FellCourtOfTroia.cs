using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90.3: The Fell Court of Troia dungeon logic.
/// </summary>
public class FellCourtOfTroia : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheFellCourtOfTroia;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheFellCourtOfTroia;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
