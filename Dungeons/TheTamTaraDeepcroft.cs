using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 16: The Tam-Tara Deepcroft dungeon logic.
/// </summary>
public class TheTamTaraDeepcroft : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.TheTamTaraDeepcroft;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheTamTaraDeepcroft;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
