using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using ff14bot.Helpers;
using RBTrust.Plugins.Trust.Extensions;
using System.Windows.Media;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    public class TheNavel : AbstractDungeon
    {
        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        static HashSet<uint> Spells = new HashSet<uint>()
        {
            651
        };

        public override async Task<bool> RunAsync()
        {
            /*
             * [12:14:39.575 V] [SideStep] Landslide [CastType][Id: 650][Omen: 9][RawCastType: 4][ObjId: 1073996108]
             *	Handled by SideStep
             * [12:15:07.346 V] [SideStep] Geocrush [CastType][Id: 651][Omen: 152][RawCastType: 2][ObjId: 1073996108]
             *	Need to follow NPC here.
             * [12:38:36.865 V] [SideStep] Weight of the Land [CastType][Id: 973][Omen: 8][RawCastType: 2][ObjId: 1073851629]
             *	Handled by SideStep
             */
            //Titan

            if (GameObjectManager.GetObjectByNPCId(1801) != null) //Titan
            {
                if (Spells.IsCasting())
                {
                    if (Spells.IsCasting())
                    {
                        sidestepPlugin.Enabled = false;
                        AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                        await MovementHelpers.GetClosestAlly.Follow();
                    }

                    sidestepPlugin.Enabled = true;
                    Logging.Write(Colors.Aquamarine, "Resetting navigation");
                    AvoidanceManager.ResetNavigation();
                }
            }

            await Coroutine.Yield();
            return false;
        }

        public override DungeonId DungeonId { get; }
    }
}
