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
    /// Lv. 79 Mt. Gulg dungeon logic.
    /// </summary>
    public class MtGulg : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.MtGulg;

        private const int ForgivenWhimsy = 8261;
        private const int ForgivenCruelty = 8260;
        private const int ForgivenRevelry = 8270;
        private const int ForgivenObscenity = 8262;

        /// <summary>
        /// Set of boss-related monster IDs.
        /// </summary>
        private static readonly HashSet<uint> BossIds = new HashSet<uint>
        {
            4385,
            7864,
            8925, // Brightsphere          :: 光明晶球
            ForgivenCruelty,
            ForgivenWhimsy,
            ForgivenObscenity,
            ForgivenRevelry,
            8299, // Forgiven Dissonance   :: 得到宽恕的失调
        };

        private static readonly HashSet<uint> Spells = new HashSet<uint>() { 15614, 15615, 15616, 15617, 15618, 15622, 15623, 15638, 15640, 15641, 15642, 15643, 15644, 15645, 15648, 15649, 16247, 16248, 16249, 16250, 16521, 16818, 16987, 16988, 16989, 17153, 18025, };
        private static readonly HashSet<uint> LumenInfinitum = new HashSet<uint>() { 16818 };
        private static readonly HashSet<uint> Exegesis = new HashSet<uint>() { 15622, 15623, 16987, 16988, 16989, };
        private static readonly HashSet<uint> RightPalm = new HashSet<uint>() { 16247, 16248 };
        private static readonly HashSet<uint> LeftPalm = new HashSet<uint>() { 16249, 16250 };
        private static readonly HashSet<uint> GoldChaser = new HashSet<uint>() { 15652, 15653, 17066 };

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.MtGulg;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            BattleCharacter forgivenCrueltyNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenCruelty)
                .FirstOrDefault(bc => bc.Distance() < 50);
            if (forgivenCrueltyNpc != null && forgivenCrueltyNpc.IsValid)
            {
                if (LumenInfinitum.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }
            }

            BattleCharacter forgivenWhimsyNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenWhimsy)
                .FirstOrDefault(bc => bc.Distance() < 50);
            if (forgivenWhimsyNpc != null && forgivenWhimsyNpc.IsValid)
            {
                SidestepPlugin.Enabled = false;

                if (Exegesis.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }
            }
            else
            {
                SidestepPlugin.Enabled = true;
            }

            BattleCharacter forgivenRevelryNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenRevelry)
                .FirstOrDefault(bc => bc.Distance() < 50);
            if (forgivenRevelryNpc != null && forgivenRevelryNpc.IsValid)
            {
                if (RightPalm.IsCasting() || LeftPalm.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }
            }

            BattleCharacter forgivenObscenityNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ForgivenObscenity)
                .FirstOrDefault(bc => bc.Distance() < 50);
            if (forgivenObscenityNpc != null && forgivenObscenityNpc.IsValid)
            {
                if (GoldChaser.IsCasting())
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 20_000, "Forgiven Obscenity Avoid");
                    while (sw.ElapsedMilliseconds < 20_000)
                    {
                        await MovementHelpers.GetClosestAlly.Follow();
                        await Coroutine.Yield();
                    }

                    sw.Stop();
                }
            }

            if (Spells.IsCasting())
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 1_500, "Enemy Spell Cast In Progress");
                await MovementHelpers.GetClosestAlly.Follow();
            }

            if (WorldManager.SubZoneId != 3000)
            {
                BossIds.ToggleSideStep(new uint[] { ForgivenObscenity });
            }
            else
            {
                BossIds.ToggleSideStep();
            }

            return false;
        }
    }
}
