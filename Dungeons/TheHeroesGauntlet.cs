using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 80.4: The Heroes' Gauntlet dungeon logic.
/// </summary>
public class TheHeroesGauntlet : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheHeroesGauntlet;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheHeroesGauntlet;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
