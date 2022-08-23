using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 50.5: The Keeper of the Lake dungeon logic.
/// </summary>
public class KeeperOfTheLake : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheKeeperOfTheLake;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheKeeperOfTheLake;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
