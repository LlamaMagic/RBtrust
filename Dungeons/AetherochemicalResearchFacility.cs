using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60.1: The Aetherochemical Research Facility dungeon logic.
/// </summary>
public class AetherochemicalResearchFacility : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
