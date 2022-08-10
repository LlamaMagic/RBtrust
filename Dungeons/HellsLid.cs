using Buddy.Coroutines;
using Clio.Common;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70.5: Hells' Lid dungeon logic.
/// </summary>
public class HellsLid : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.HellsLid;

    private static readonly HashSet<uint> HellOfWater = new()
    {
        11541,
        10192,
    };

    private static readonly HashSet<uint> HellOfWaste2 = new()
    {
        10194,
        10193,
    };

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        11541,
        10192, // Hell of Water by Genbu
        10193,
        10194, // Hell of Waste by Genbu
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (HellOfWater.IsCasting() || HellOfWaste2.IsCasting())
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 4_000, "Hell of Water/Waste");
            await CommonTasks.MoveTo(MathEx.GetRandomPointInCircle(Core.Player.Location, 3f));
            await Coroutine.Yield();

            await Coroutine.Sleep(3_000);

            if (ActionManager.IsSprintReady)
            {
                ActionManager.Sprint();
                await Coroutine.Wait(1_000, () => !ActionManager.IsSprintReady);
            }

            await Coroutine.Sleep(1_000);
            await Coroutine.Yield();
        }

        return false;
    }
}
