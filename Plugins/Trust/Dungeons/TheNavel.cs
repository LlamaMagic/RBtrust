using Buddy.Coroutines;
using ff14bot.Helpers;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 34: The Navel dungeon logic.
    /// </summary>
    public class TheNavel : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheNavel;

        private const int Titan = 1801;

        private static readonly HashSet<uint> Spells = new HashSet<uint>()
        {
            651,
        };

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheNavel;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            /*
             * [12:14:39.575 V] [SideStep] Landslide [CastType][Id: 650][Omen: 9][RawCastType: 4][ObjId: 1073996108]
             *    Handled by SideStep
             * [12:15:07.346 V] [SideStep] Geocrush [CastType][Id: 651][Omen: 152][RawCastType: 2][ObjId: 1073996108]
             *    Need to follow NPC here.
             * [12:38:36.865 V] [SideStep] Weight of the Land [CastType][Id: 973][Omen: 8][RawCastType: 2][ObjId: 1073851629]
             *    Handled by SideStep
             */

            if (GameObjectManager.GetObjectByNPCId(Titan) != null)
            {
                if (Spells.IsCasting())
                {
                    if (Spells.IsCasting())
                    {
                        SidestepPlugin.Enabled = false;
                        AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                        await MovementHelpers.GetClosestAlly.Follow();
                    }

                    SidestepPlugin.Enabled = true;
                    Logging.Write(Colors.Aquamarine, "Resetting navigation");
                    AvoidanceManager.ResetNavigation();
                }
            }

            await Coroutine.Yield();

            return false;
        }
    }
}
