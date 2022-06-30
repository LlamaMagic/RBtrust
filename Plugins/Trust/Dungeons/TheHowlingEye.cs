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
using System.Windows.Media;
using ff14bot.Enums;
using RBTrust.Plugins.Trust.Extensions;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    public class TheHowlingEye : AbstractDungeon
    {
        public override DungeonId DungeonId { get; }

        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        static HashSet<uint> Spells = new HashSet<uint>() {651};

        public override async Task<bool> RunAsync()
        {
            /*

             Name:Slipstream SpellId:659
             Mistral Song, IsSpell: True, ActionId: 667
             Slipstream, IsSpell: True, ActionId: 659
             Name: Aerial Blast SpellId: 662
             Name: Mistral Shriek SpellId: 661

             */

            IEnumerable<BattleCharacter> Garuda = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 26 && r.NpcId == 1644 && r.CurrentHealthPercent < 100);

            if (Garuda.Any())
            {
                if (GameObjectManager.GetObjectByNPCId(1644) != null) //Garuda
                {
                    HashSet<uint> MistralSong = new HashSet<uint>() {667, 660};
                    if (MistralSong.IsCasting())
                    {
                        await MovementHelpers.GetClosestDps.Follow();
                        await Coroutine.Wait(10000, () => !MistralSong.IsCasting());
                        if (!MistralSong.IsCasting())
                        {
                            await MovementHelpers.Spread(2000, 5);
                        }
                    }

                    HashSet<uint> Slipstream = new HashSet<uint>() {659};
                    if (Slipstream.IsCasting() && !GeneralHelpers.IsTankClass())
                    {
                        await MovementHelpers.Spread(5000, 5);
                    }

                    HashSet<uint> MistralShriek = new HashSet<uint>() {661};
                    if (MistralShriek.IsCasting())
                    {
                        await MovementHelpers.GetClosestDps.Follow();
                    }

                    if (!Spells.IsCasting())
                    {
                        if (!sidestepPlugin.Enabled)
                        {
                            await Coroutine.Sleep(500);
                            sidestepPlugin.Enabled = true;
                        }
                    }
                }
            }

            await Coroutine.Yield();
            return false;
        }
    }
}
