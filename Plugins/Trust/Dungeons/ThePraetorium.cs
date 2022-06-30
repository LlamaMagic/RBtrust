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
    public class ThePraetorium : AbstractDungeon
    {
        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        static HashSet<uint> Spells = new HashSet<uint>()
        {
            // Nero
            // Augmented Suffering
            1156,
            7607,
            8492,
            21080,
            21101,
            28622,
            28476,
            // Augmented Shater
            1158,
            7609,
            8494,
            28477,
            28619,

            //Gaius
            // Festina Lente
            19657,
            19774,
            20107,
            28493
        };

        public override async Task<bool> RunAsync()
        {
            /*
Augmented Shatter, stack

Augmented suffering, stack

             */

            // Nero 2135
            if (GameObjectManager.GetObjectByNPCId(2135) != null)
            {
                // AugmentedSuffering 1156, 7607, 8492, 21080, 21101, 28622, 28476 ]
                HashSet<uint> AugmentedSuffering = new HashSet<uint>()
                {
                    1156,
                    7607,
                    8492,
                    21080,
                    21101,
                    28622,
                    28476
                };
                if (AugmentedSuffering.IsCasting())
                {
                    //sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }

                // AugmentedShatter 1158, 7609, 8494, 28477, 28619 ]
                HashSet<uint> AugmentedShatter = new HashSet<uint>()
                {
                    1158,
                    7609,
                    8494,
                    28477,
                    28619
                };
                if (AugmentedShatter.IsCasting())
                {
                    //sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }
            }

            /* Gaius 2136

                Terminus Est [CastType][Id: 28488][Omen: 2][RawCastType: 12][ObjId: 1073928578]
                    Sidestep

                Festina Lente
                    19657, 19774, 20107, 28493

                */
            if (GameObjectManager.GetObjectByNPCId(2136) != null)
            {
                // Festina Lente 19657, 19774, 20107, 28493  ]
                HashSet<uint> FestinaLente = new HashSet<uint>() {19657, 19774, 20107, 28493};
                if (FestinaLente.IsCasting())
                {
                    //sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }
            }


            if (!Spells.IsCasting())
            {
                if (!sidestepPlugin.Enabled)
                {
                    sidestepPlugin.Enabled = true;
                }
            }

            await Coroutine.Yield();
            return false;
        }

        public override DungeonId DungeonId { get; }
    }
}
