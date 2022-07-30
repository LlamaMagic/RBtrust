using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 17: Copperbell Mines dungeon logic.
/// </summary>
public class CopperbellMines : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.CopperbellMines;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.CopperbellMines;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        return false;
    }
}
