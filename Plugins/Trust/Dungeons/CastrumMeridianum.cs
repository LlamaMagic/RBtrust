using Buddy.Coroutines;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 50.1: Castrum Meridianum dungeon logic.
    /// </summary>
    public class CastrumMeridianum : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.CastrumMeridianum;

        private const int LiviaSasJunius = 2118;

        private static readonly HashSet<uint> BossIds = new HashSet<uint>
        {
            LiviaSasJunius,
        };

        private static readonly Stopwatch StackStopwatch = new Stopwatch();

        // BOSS MECHANIC SPELLIDS

        // B1
        // The Black Eft
        //  Not Needed

        // B2
        // MAGITEK VANGUARD F-1
        //  Thermobaric Strike  28778 (Stack) (not used but tested)
        //  Thermbiotic Charge  28779 (Floor, run away from)
        //  Cement Drill        28785 (AOE)
        //  Hyper Charge        28780 (AOE)

        // B3
        // Livia Sas Junius
        //  Roundhouse           28786 (Stack)
        //  Aglaea               28798 (TankBuster)
        //  Infinite Reach       28791 (Stack)
        //  Discharge            28794, 28790, 28787 (Stack)
        //  Angry Salamader      28797 (Stack)
        //  Thermobaric Charge   29356 (Stack)
        //
        // Debug Info - It runs away and gets stuck on Edge on both of These
        //
        // TRASH WITH NO OMEN
        //

        // GENERIC MECHANICS
        private static readonly HashSet<uint> Spells = new HashSet<uint>()
        {
            28778, 28779, 28786, 28791, 28793,
            28797, 28790, 29356, 28787, 29357,
            29356,
        };

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.CastrumMeridianum;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            BattleCharacter liviaNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: LiviaSasJunius).FirstOrDefault(bc => bc.Distance() < 50);
            if (liviaNpc != null && liviaNpc.IsValid)
            {
                if (Spells.IsCasting())
                {
                    SidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }

                if (StackStopwatch.ElapsedMilliseconds > 2000)
                {
                    StackStopwatch.Reset();
                    SidestepPlugin.Enabled = true;
                }
            }

            await Coroutine.Yield();
            return false;
        }
    }
}
