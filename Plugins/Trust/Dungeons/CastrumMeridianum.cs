using Buddy.Coroutines;
using ff14bot.Managers;
using ff14bot.Objects;
using RBTrust.Plugins.Trust.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;


namespace Trust.Dungeons
{
    public class CastrumMeridianum : AbstractDungeon
    {
        public override DungeonId DungeonId { get; }

        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();
        private static Stopwatch STsw = new Stopwatch(); //ST = Stack

        /// <summary>
        /// 1043 Zone Id.
        /// </summary>
        /// <inheritdoc/>

        // BOSS MECHANIC SPELLIDS

        // B1
        // The Black Eft
        // Not Needed

        // B2
        // MAGITEK VANGUARD F-1
        //
        //	 			Thermobaric Strike 	28778 (Stack) (not used but tested)
        //				Thermbiotic Charge  28779 (Floor  run away from)
        //				Cement Drill 		28785 (AOE)
        //				Hyper Charge		28780 (AOE)

        // B3
        // Livia Sas Junius
        //				Roundhouse				28786 (Stack)
        //				Aglaea					28798 (TankBuster)
        //				Infinite Reach			28791 (Stack)
        //				Discharge				28794,28790,28787 (Stack)
        //				Angry Salamader			28797 (Stack)
        //				Thermoibaric Charge     29356 (Stack)
        //
        //	Debug Info - It runs away and gets stuck on Edge on both of These
        //
        // TRASH WITH NO OMEN
        //

        // GENERIC MECHANICS
        static HashSet<uint> Spells = new HashSet<uint>()
        {
            28778,
            28779,
            28786,
            28791,
            28793,
            28797,
            28790,
            29356,
            28787,
            29357,
            29356
        };

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            IEnumerable<BattleCharacter> LiviasasJunius = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 2118); // Livia sas Junius

            // Livia sas Junius 2118
            if (LiviasasJunius.Any())
            {

                if (Spells.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }

                if (STsw.ElapsedMilliseconds > 2000)
                {
                    STsw.Reset();
                    sidestepPlugin.Enabled = true;
                }
            }

            await Coroutine.Yield();
            return false;
        }
    }
}
