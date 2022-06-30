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
using ff14bot.Navigation;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons
{
    public class HallofNoviceArena : AbstractDungeon
    {
        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        public override DungeonId DungeonId { get; }

        static HashSet<uint> Spells = new HashSet<uint>() { };

        public override async Task<bool> RunAsync()
        {
            IEnumerable<BattleCharacter> fisters = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 4786 && r.IsAlive); // Quick-fisted Training Partner
            IEnumerable<BattleCharacter> tamedJackal = GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(
                r => !r.IsMe && r.Distance() < 50 && r.NpcId == 4787 && r.IsAlive); // Tamed Jackal

            // Quick-fisted Training Partner 4786
            if (fisters.Any())
            {
                var fistNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(4786)
                    .Where(obj => obj.IsVisible && obj.CurrentHealth > 1);
                foreach (var fister in fistNpc)
                {
                    while (fister.TargetGameObject != Core.Me)
                    {
                        if (fister.Distance2D(Core.Me.Location) >= 20)

                        {
                            Logging.WriteDiagnostic($"Hunting those fisters.");
                            var _target = fister.Location;
                            Navigator.PlayerMover.MoveTowards(_target);
                            while (_target.Distance2D(Core.Me.Location) >= 20)
                            {
                                Navigator.PlayerMover.MoveTowards(_target);
                                await Coroutine.Sleep(100);
                            }

                            Navigator.PlayerMover.MoveStop();
                        }

                        if (Core.Me.CurrentJob == ClassJobType.Marauder || Core.Me.CurrentJob == ClassJobType.Warrior)
                        {
                            Logging.WriteDiagnostic($"Casting Tomahawk.");
                            ActionManager.DoAction(46, fister);
                            await Coroutine.Sleep(1500);
                        }

                        if (Core.Me.CurrentJob == ClassJobType.Gladiator || Core.Me.CurrentJob == ClassJobType.Paladin)
                        {
                            Logging.WriteDiagnostic($"Casting Shield Lob.");
                            ActionManager.DoAction(24, fister);
                            await Coroutine.Sleep(1500);
                        }

                        if (Core.Me.CurrentJob == ClassJobType.DarkKnight)
                        {
                            Logging.WriteDiagnostic($"Casting Unmend");
                            ActionManager.DoAction(3624, fister);
                            await Coroutine.Sleep(1500);
                        }
                    }
                }
            }

            // Tamed Jackal 4787
            if (tamedJackal.Any())
            {
                var fistNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(4787)
                    .Where(obj => obj.IsVisible && obj.CurrentHealth > 1 && obj.IsAlive);
                foreach (var fister in fistNpc)
                {
                    while (fister.TargetGameObject != Core.Me)
                    {
                        if (fister.Distance2D(Core.Me.Location) >= 20)

                        {
                            Logging.WriteDiagnostic($"Hunting those Jackals.");
                            var _target = fister.Location;
                            Navigator.PlayerMover.MoveTowards(_target);
                            while (_target.Distance2D(Core.Me.Location) >= 20)
                            {
                                Navigator.PlayerMover.MoveTowards(_target);
                                await Coroutine.Sleep(100);
                            }

                            Navigator.PlayerMover.MoveStop();
                        }

                        if (Core.Me.CurrentJob == ClassJobType.Marauder || Core.Me.CurrentJob == ClassJobType.Warrior)
                        {
                            Logging.WriteDiagnostic($"Casting Tomahawk.");
                            ActionManager.DoAction(46, fister);
                            await Coroutine.Sleep(1500);
                        }

                        if (Core.Me.CurrentJob == ClassJobType.Gladiator || Core.Me.CurrentJob == ClassJobType.Paladin)
                        {
                            Logging.WriteDiagnostic($"Casting Shield Lob.");
                            ActionManager.DoAction(24, fister);
                            await Coroutine.Sleep(1500);
                        }

                        if (Core.Me.CurrentJob == ClassJobType.DarkKnight)
                        {
                            Logging.WriteDiagnostic($"Casting Unmend");
                            ActionManager.DoAction(3624, fister);
                            await Coroutine.Sleep(1500);
                        }
                    }
                }
            }


            await Coroutine.Yield();
            return false;
        }
    }
}
