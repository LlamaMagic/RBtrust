using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 77: Malikah's Well dungeon logic.
/// </summary>
public class MalikahsWell : AbstractDungeon
{
    private const int GreaterArmadillo = 8252;
    private const int AmphibiousTalos = 8250;
    private const int Storge = 8249;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.MalikahsWell;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.MalikahsWell;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        15590, 15591, 15592, 15593, 15602, 15605, 15606, 15607, 15610, 15609,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
