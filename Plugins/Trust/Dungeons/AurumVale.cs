using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Helpers;
using ff14bot.Navigation;
using LlamaLibrary.Helpers;
using System.Collections.Generic;
using Trust.Data;

namespace Trust.Dungeons
{
    public class AurumVale : AbstractDungeon
    {
        public override DungeonId DungeonId { get; }

        public override async Task<bool> RunAsync()
        {
            IEnumerable<BattleCharacter> Lockhart = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 1534); // Lockhart

            IEnumerable<BattleCharacter> MisersMistress = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 1532); // Miser's Mistress


            //Lockhart 1534
            if (Lockhart.Any())
            {
                if (Core.Me.HasAura(302) && Core.Me.GetAuraById(302).Value > 2) //Gold Lung
                {
                    var npc = GameObjectManager
                        .GetObjectsByNPCIds<GameObject>(new uint[] {2002647, 2002648, 2002649, 2000778})
                        .OrderBy(i => i.Distance()).FirstOrDefault();

                    if (npc != default(GameObject))
                    {
                        Logging.Write("We have aura, moving to Morbol Fruit.");
                        if (await Navigation.FlightorMove(npc.Location))
                        {
                            npc.Interact();
                            await Coroutine.Wait(10000, () => !Core.Me.HasAura(302));
                        }
                        else
                        {
                            //Couldn't get to the npc
                        }
                    }
                    else
                    {
                        //Can't find npc
                    }
                }
            }

            //Miser's Mistress 1532
            if (MisersMistress.Any())
            {
                if ( Core.Me.HasAura(303) && Core.Me.GetAuraById(303).Value > 2) //Burrs
                {
                    var npc = GameObjectManager.GetObjectsByNPCIds<GameObject>(new uint[]
                        {
                            2002654, 2002655, 2002656, 2002657, 2002658, 2002659, 2002660, 2002661, 2002662, 2002663
                        })
                        .OrderBy(i => i.Distance()).FirstOrDefault();

                    if (npc != default(GameObject))
                    {
                        Logging.Write("We have aura, moving to Morbol Fruit.");
                        if (await Navigation.FlightorMove(npc.Location))
                        {
                            npc.Interact();
                            await Coroutine.Wait(10000, () => !Core.Me.HasAura(303));
                        }
                        else
                        {
                            //Couldn't get to the npc
                        }
                    }
                    else
                    {
                        //Can't find npc
                    }
                }
            }


            await Coroutine.Yield();
            return false;
        }
    }
}
