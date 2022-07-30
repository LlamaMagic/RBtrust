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
    public override async Task<bool> RunAsync()
    {
        return false;
    }
}
