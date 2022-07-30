using Buddy.Coroutines;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 50.5: The Wanderer's Palace dungeon logic.
/// </summary>
public class TheWanderersPalace : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.TheWanderersPalace;

    private const int TonberryStalker = 1556;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        GameObject tStalker = GameObjectManager.GetObjectsByNPCId<GameObject>(NpcId: TonberryStalker)
            .FirstOrDefault(bc => bc.Distance() < 10 && bc.IsVisible);

        if (tStalker != null)
        {
            AvoidanceManager.AddAvoidObject<GameObject>(() => true, 8f, tStalker.ObjectId);
        }

        await Coroutine.Yield();

        return false;
    }
}
