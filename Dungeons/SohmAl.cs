using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;

namespace Trust.Dungeons;

/// <summary>
/// Abstract starting point for implementing specialized dungeon logic.
/// </summary>
public class SohmAl : AbstractDungeon
{
    private readonly HashSet<uint> fireball = new() { 3928 };

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.SohmAl;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <summary>
    /// Gets spell IDs to follow-dodge while any contained spell is casting.
    /// </summary>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <summary>
    /// Executes dungeon logic.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (fireball.IsCasting())
        {
            uint[] ids = GameObjectManager.GetObjectsByNPCId(2005287).Select(i => i.ObjectId).ToArray();
            AvoidanceManager.AddAvoidObject<GameObject>(() => true, 6f, ids);
        }

        return false;
    }
}
