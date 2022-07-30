using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 28: Haukke Manor dungeon logic.
/// </summary>
public class HaukkeManor : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.HaukkeManor;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.HaukkeManor;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        return false;
    }
}
