using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using Clio.Utilities;
using ff14bot.Helpers;
using System.Diagnostics;
using ff14bot.Navigation;
using System.Windows.Media;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    public class TheStoneVigil : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheStoneVigil;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheStoneVigil;

        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        private static DateTime SwingeTimestamp = DateTime.MinValue;
        private static readonly int SwingeDuration = 15_000;

        private static DateTime lionsBreathTimestamp = DateTime.MinValue;
        private static readonly int lionsBreathDuration = 10_000;

        private static DateTime rimeWreathTimestamp = DateTime.MinValue;
        private static readonly int rimeWreathgDuration = 5_000;

        private static DateTime frostBreathTimestamp = DateTime.MinValue;
        private static readonly int frostBreathDuration = 3_000;

        public override async Task<bool> RunAsync()
        {
            IEnumerable<BattleCharacter> ChudoYudo = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 1677); // Chudo-Yudo

            IEnumerable<BattleCharacter> Koshchei = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 1678); // Koshchei

            IEnumerable<BattleCharacter> Isgebind = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 1680); // Isgebind 1680


            // Chudo-Yudo  1677
            if (ChudoYudo.Any())
            {
                GameObject ChudoYudoObj =
                    GameObjectManager.GetObjectsByNPCId(1677).FirstOrDefault(obj => obj.IsTargetable);


                HashSet<uint> Swinge = new HashSet<uint>() {903};
                if (Swinge.IsCasting() && SwingeTimestamp.AddMilliseconds(SwingeDuration) < DateTime.Now)

                {
                    Vector3 location = ChudoYudoObj.Location;
                    uint objectId = ChudoYudoObj.ObjectId;

                    SwingeTimestamp = DateTime.Now;
                    Stopwatch SwingeTimer = new Stopwatch();
                    SwingeTimer.Restart();

                    AvoidanceManager.AddAvoidUnitCone<GameObject>(
                        canRun: () => SwingeTimer.IsRunning && SwingeTimer.ElapsedMilliseconds < SwingeDuration,
                        objectSelector: (obj) => obj.ObjectId == objectId,
                        leashPointProducer: () => location,
                        leashRadius: 80f,
                        rotationDegrees: 0f,
                        radius: 90f,
                        arcDegrees: 60f);
                }

                HashSet<uint> lionsBreath = new HashSet<uint>() {902};
                if (lionsBreath.IsCasting() && lionsBreathTimestamp.AddMilliseconds(lionsBreathDuration) < DateTime.Now)
                {
                    Vector3 location = ChudoYudoObj.Location;
                    uint objectId = ChudoYudoObj.ObjectId;

                    lionsBreathTimestamp = DateTime.Now;
                    Stopwatch lionsBreathTimer = new Stopwatch();
                    lionsBreathTimer.Restart();

                    // Create an AOE avoid for the frost wreath around the boss
                    AvoidanceManager.AddAvoidObject<GameObject>(
                        canRun: () =>
                            lionsBreathTimer.IsRunning && lionsBreathTimer.ElapsedMilliseconds < lionsBreathDuration,
                        radius: 5f,
                        unitIds: objectId);

                    AvoidanceManager.AddAvoidUnitCone<GameObject>(
                        canRun: () =>
                            lionsBreathTimer.IsRunning && lionsBreathTimer.ElapsedMilliseconds < lionsBreathDuration,
                        objectSelector: (obj) => obj.ObjectId == objectId,
                        leashPointProducer: () => location,
                        leashRadius: 40f,
                        rotationDegrees: 0f,
                        radius: 25f,
                        arcDegrees: 180f);
                }

                if (!Swinge.IsCasting() && !lionsBreath.IsCasting())
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                }
            }

            // Koshchei 1678
            if (Koshchei.Any())
            {
                HashSet<uint> Typhoon = new HashSet<uint>() {28730};
                if (Typhoon.IsCasting())
                {
                    var ids = GameObjectManager.GetObjectsByNPCId(9910).Select(i => i.ObjectId).ToArray();
                    AvoidanceManager.AddAvoidObject<GameObject>(() => true, 5.5f, ids);
                }
            }

            // Isgebind 1680
            if (Isgebind.Any())
            {
                GameObject isgebind = GameObjectManager.GetObjectsByNPCId(1680).FirstOrDefault(obj => obj.IsTargetable);
                IEnumerable<BattleCharacter> isgebindCast = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                    r => !r.IsMe && r.Distance() < 50 && r.NpcId == 1680 && r.IsCasting);

                HashSet<uint> Cauterize = new HashSet<uint>() {1026};
                if (Cauterize.IsCasting())
                {
                    //AvoidanceManager.AddAvoidObject<GameObject>(()=> true, 5f, isgebindCast);


                    Vector3 location = new Vector3("-0.0195615, 0.04040873, -247.211");
                    while (location.Distance2D(Core.Me.Location) > 1)
                    {
                        Navigator.PlayerMover.MoveTowards(location);
                        await Coroutine.Sleep(30);
                        Navigator.PlayerMover.MoveStop();
                    }
                }

                HashSet<uint> FrostBreath = new HashSet<uint>() {1022};
                if (FrostBreath.IsCasting() && frostBreathTimestamp.AddMilliseconds(frostBreathDuration) < DateTime.Now)
                {
                    Vector3 location = isgebind.Location;
                    uint objectId = isgebind.ObjectId;

                    frostBreathTimestamp = DateTime.Now;
                    Stopwatch frostBreathTimer = new Stopwatch();
                    frostBreathTimer.Restart();

                    // Create cone avoid to avoid the frost breath
                    AvoidanceManager.AddAvoidUnitCone<GameObject>(
                        canRun: () =>
                            frostBreathTimer.IsRunning && frostBreathTimer.ElapsedMilliseconds < frostBreathDuration,
                        objectSelector: (obj) => obj.ObjectId == objectId,
                        leashPointProducer: () => location,
                        leashRadius: 40f,
                        rotationDegrees: 0f,
                        radius: 25f,
                        arcDegrees: 180f);
                }

                // It doesn't look like this is dodgeable. Even getting the whole room away you still take damage
                /*
                HashSet<uint> RimeWreath = new HashSet<uint>() {1025};
                if (RimeWreath.IsCasting() && rimeWreathTimestamp.AddMilliseconds(rimeWreathgDuration) < DateTime.Now)
                {
                    Vector3 location = isgebind.Location;
                    uint objectId = isgebind.ObjectId;

                    rimeWreathTimestamp = DateTime.Now;
                    Stopwatch rimeWreathTimer = new Stopwatch();
                    rimeWreathTimer.Restart();

                    // Create an AOE avoid for the frost wreath around the boss
                    AvoidanceManager.AddAvoidObject<GameObject>(
                        canRun: () => rimeWreathTimer.IsRunning && rimeWreathTimer.ElapsedMilliseconds < rimeWreathgDuration,
                        radius: 30f,
                        unitIds: objectId);
                }*/
            }


            return false;
        }
    }
}
