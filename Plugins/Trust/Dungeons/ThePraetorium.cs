using Buddy.Coroutines;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 50: The Praetorium dungeon logic.
    /// </summary>
    public class ThePraetorium : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.NONE;

        private const int Nero = 2135;
        private const int Gaius = 2136;

        private static readonly HashSet<uint> BossIds = new HashSet<uint>
        {
            Nero, Nero,
        };

        private static readonly HashSet<uint> Spells = new HashSet<uint>()
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

            // Festina Lente
            19657,
            19774,
            20107,
            28493,
        };

        // Augmented Suffering, stack
        private static readonly HashSet<uint> AugmentedSuffering = new HashSet<uint>()
        {
            1156,
            7607,
            8492,
            21080,
            21101,
            28622,
            28476,
        };

        // Augmented Shatter, stack
        private static readonly HashSet<uint> AugmentedShatter = new HashSet<uint>()
        {
            1158,
            7609,
            8494,
            28477,
            28619,
        };

        private static readonly HashSet<uint> FestinaLente = new HashSet<uint>()
        {
            19657, 19774, 20107, 28493,
        };

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.ThePraetorium;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            if (GameObjectManager.GetObjectByNPCId(Nero) != null)
            {
                if (AugmentedSuffering.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }

                if (AugmentedShatter.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }
            }

            if (GameObjectManager.GetObjectByNPCId(Gaius) != null)
            {
                if (FestinaLente.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestAlly.Follow();
                }
            }

            if (!Spells.IsCasting())
            {
                SidestepPlugin.Enabled = true;
            }

            await Coroutine.Yield();

            return false;
        }
    }
}
