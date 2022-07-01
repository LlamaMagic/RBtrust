using Buddy.Coroutines;
using Clio.Common;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 70.5: Hells' Lid dungeon logic.
    /// </summary>
    public class HellsLid : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.HellsLid;

        // 532, 1837, 2794, 5445, 7931, 9076, 9338, 9490, 2441
        private static readonly HashSet<uint> Spells = new HashSet<uint>()
        {
            // 1st boss 100 tonze swing
            // 10176 liquid capace (constant spewing attack, needs to run away)
            // 2nd boss Reapders gale
            // 10599. 10187 <45.62811, -26, -105.941> or Current XYZ: <50.26293, -26, -111.4103>
            11541,
            10192, // Hell of Water by Genbu
            10193,
            10194, // Hell of Waste by Genbu

            // ,10196,10197 //Sinister Tide (Arrow Mechanic)
        };

        private static readonly HashSet<uint> HellOfWater = new HashSet<uint>()
        {
            11541,
            10192,
        };

        private static readonly HashSet<uint> HellOfWaste2 = new HashSet<uint>()
        {
            10194,
            10193,
        };

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.NONE;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            if (HellOfWater.IsCasting() || HellOfWaste2.IsCasting())
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 4000, "Hell of Water/Waste");
                await CommonTasks.MoveTo(MathEx.GetRandomPointInCircle(Core.Player.Location, 3f));
                await Coroutine.Yield();

                await Coroutine.Sleep(3000);

                if (ActionManager.IsSprintReady)
                {
                    ActionManager.Sprint();
                    await Coroutine.Wait(1000, () => !ActionManager.IsSprintReady);
                }

                await Coroutine.Sleep(1000);
                await Coroutine.Yield();
            }

            if (Spells.IsCasting())
            {
                await MovementHelpers.GetClosestAlly.Follow();
                await Coroutine.Yield();
            }

            return false;
        }
    }
}
