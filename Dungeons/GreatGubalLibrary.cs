using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 59: The Great Gubal Library dungeon logic.
/// </summary>
public class GreatGubalLibrary : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheGreatGubalLibrary;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheGreatGubalLibrary;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
