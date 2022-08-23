using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70.2: The Drowned City of Skalla dungeon logic.
/// </summary>
public class DrownedCityOfSkalla : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheDrownedCityOfSkalla;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheDrownedCityOfSkalla;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
