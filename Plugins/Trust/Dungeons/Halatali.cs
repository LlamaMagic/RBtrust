using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using Trust.Data;
using Trust.Helpers;


namespace Trust.Dungeons
{
    public class Halatali : AbstractDungeon
    {
        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        public override DungeonId DungeonId { get; }

        static HashSet<uint> Spells = new HashSet<uint>() { };

        public override async Task<bool> RunAsync()
        {
            IEnumerable<BattleCharacter> thunderclapGuivre = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(
                    r => !r.IsMe && r.Distance() < 100 && r.NpcId == 1196); // Thunderclap Guivre

            IEnumerable<BattleCharacter> LightningPool = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 100 && r.NpcId == 2001648 && !r.IsVisible); // Lightning Pool


            // Thunderclap Guivre 1196
            if (thunderclapGuivre.Any())
            {
                if (!GameObjectManager.GetObjectByNPCId(2001648).IsVisible)
                {
                    var avoids = new List<(Vector3 Location, float Radius)>()
                    {
                        (new Vector3(-177.9965f, -14.69446f, -133.0435f), 25f),
                        (new Vector3(-189.0614f, -15.30659f, -157.837f), 15f),
                        (new Vector3(-204.8858f, -15.06509f, -117.6959f), 20f),
                    };

                    foreach (var circle in avoids)
                    {
                        AvoidanceManager.AddAvoidLocation(
                            () => true,
                            circle.Radius,
                            () => circle.Location
                        );
                    }
                }

                if (GameObjectManager.GetObjectByNPCId(2001648).IsVisible)
                {
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                }
            }


            await Coroutine.Yield();
            return false;
        }
    }
}
