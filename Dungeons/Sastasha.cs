using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 15: Sastasha dungeon logic.
/// </summary>
public class Sastasha : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.Sastaha;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Sastasha;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        return false;
    }
}
