using ff14bot.Managers;
using ff14bot.Objects;
using RBTrust.Plugins.Trust.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 75 The Qitana Ravel dungeon logic.
    /// </summary>
    public class TheQitanaRavel : AbstractDungeon
    {
        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        private readonly HashSet<uint> spellCastIds = new HashSet<uint>()
        {
            15918,
            15916,
            15917,
            17223,
            15498,
            15500,
            15725,
            15501,
            15503,
            15504,
            15509,
            15510,
            15511,
            15512,
            17213,
            15570,
            16263,
            14730,
            15514,
            15516,
            15517,
            15518,
            15519,
            15520,
            16923,
            15523,
            15527,
            15522,
            15526,
            15525,
            15524,
        };

        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheQitanaRavel;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheQitanaRavel;

        // removed trash mob / no stack boss spells
        //          15926 Forgiven Violence - SinSpitter
        //          16260 Echo of Qitana - Self-destruct
        //          15502 Lozatl - Heat Up
        //          15499 Lozatl - Lozatl's Scorn

        // not sure if can detect these two as separate spells?
        //          15524 Eros - Confession of Faith (Stack)
        //          15521 Eros - Confession of Faith (Spread)

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            IEnumerable<BattleCharacter> lozatl = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 8231); // Lozatl
            IEnumerable<BattleCharacter> batsquatch = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 8232); // Batsquatch
            IEnumerable<BattleCharacter> eros = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 8233); // Eros

            // Lozatl 8231
            if (lozatl.Any())
            {
                HashSet<uint> HeatUp = new HashSet<uint>() {15502, 15501};
                if (HeatUp.IsCasting())
                {
                    //sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }

                HashSet<uint> LozatlsScorn = new HashSet<uint>() {15499};
                if (LozatlsScorn.IsCasting())
                {
                    //sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }

                HashSet<uint> LozatlsFury = new HashSet<uint>() {15503, 15504};
                if (LozatlsFury.IsCasting())
                {
                    //sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }

                HashSet<uint> RonkanLight = new HashSet<uint>() {15725, 15500};
                if (RonkanLight.IsCasting())
                {
                    //sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }
            }

            // Batsquatch 8232
            if (batsquatch.Any())
            {
                HashSet<uint> Soundwave = new HashSet<uint>() {15506};
                if (Soundwave.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }
            }

            // Eros 8233
            if (eros.Any())
            {
                HashSet<uint> ConfessionofFaith = new HashSet<uint>()
                {
                    15521,
                    15522,
                    15523,
                    15524,
                    15525,
                    15526,
                    15527
                };
                if (ConfessionofFaith.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await MovementHelpers.GetClosestDps.Follow();
                }

                if (!spellCastIds.IsCasting())
                {
                    if (!sidestepPlugin.Enabled)
                    {
                        sidestepPlugin.Enabled = true;
                    }
                }
            }

            if (spellCastIds.IsCasting())
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 2500,
                    "Follow/Stack Mechanic In Progress");
                await MovementHelpers.GetClosestAlly.Follow();
            }

            return false;
        }
    }
}
