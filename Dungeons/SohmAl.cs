using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Abstract starting point for implementing specialized dungeon logic.
/// </summary>
public class SohmAl : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.SohmAl;

    /// <summary>
    /// Gets <see cref="DungeonId"/> for this dungeon.
    /// </summary>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <summary>
    /// Gets a handle to signal the combat routine should not use certain features (e.g., prevent CR from moving).
    /// </summary>
    protected CapabilityManagerHandle CapabilityHandle { get; } = CapabilityManager.CreateNewHandle();

    /// <summary>
    /// Gets spell IDs to follow-dodge while any contained spell is casting.
    /// </summary>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    // Fireball
    private readonly HashSet<uint> fireball = new() { 3928 };

    /// <summary>
    /// Executes dungeon logic.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (fireball.IsCasting())
        {
            var ids = GameObjectManager.GetObjectsByNPCId(2005287).Select(i => i.ObjectId).ToArray();
            AvoidanceManager.AddAvoidObject<GameObject>(() => true, 6f, ids);
        }

        return false;
    }
}
