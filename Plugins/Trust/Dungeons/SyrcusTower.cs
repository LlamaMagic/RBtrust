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
using Trust.Data;
using Trust.Dungeons;
using Trust.Extensions;

namespace Trust.Dungeons
{
    public class SyrcusTower : AbstractDungeon
    {
        public override async Task<bool> RunAsync()
        {
            // NOT TESTED

            #region Spell Filters

            /// 532, 1837, 2794, 5445, 7931, 9076, 9338, 9490, 2441

            HashSet<uint> Spells = new HashSet<uint>()
            {
                2441,
                12461,
                2361,
                12214,
                3412,
                4198,
                5254,
                5253,
                2359,
                3413,
                2317,
                1730,
                1731,
                1748,
                2347,
                5253,
                2359,
                11928
            };

            #endregion

            #region Custom Mechanics

            /// Therion (至大灾兽)
            /// 2441                                :: CurtainCall
            HashSet<uint> CurtainCall = new HashSet<uint>() {2441, 12461};
            if (CurtainCall.IsCasting())
            {
                await Coroutine.Sleep(4000);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 12000)
                {
                    Core.Me.ClearTarget();
                    //Logging.Write(Colors.Aquamarine, $"Curtain Call Handling Going to Ice");
                    // PartyManager.VisibleMembers.Where(x => !x.IsMe &&x.BattleCharacter.IsAlive).FirstOrDefault().BattleCharacter.Location
                    if (GameObjectManager.GetObjectByNPCId(2820) != null)
                    {
                        while (Core.Me.Location.Distance2D(GameObjectManager.GetObjectByNPCId(2820).Location) > 0.8)
                        {
                            MovementManager.SetFacing(GameObjectManager.GetObjectByNPCId(2820).Location);
                            MovementManager.MoveForwardStart();
                            await Coroutine.Sleep(100);
                            MovementManager.MoveStop();
                        }
                    }

                    //await Coroutine.Sleep(1000);
                    await Coroutine.Yield();
                }

                sw.Stop();
            }

            /// Therion (至大灾兽)
            /// 2441                                :: Ancient Quaga
            HashSet<uint> AncientQuaga = new HashSet<uint>()
            {
                2361,
                12214,
                3412,
                4198,
                5254,
                5253,
                2359,
                3413
            };
            if (AncientQuaga.IsCasting())
            {
                await Coroutine.Sleep(2000);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //Logging.Write(Colors.Aquamarine, $"Ancient Quaga Handling");

                while (sw.ElapsedMilliseconds < 7000)
                {
                    Core.Me.ClearTarget();
                    if (GameObjectManager.GetObjectByNPCId(2004354).IsVisible == true)
                    {
                        while (Core.Me.Location.Distance2D(GameObjectManager.GetObjectByNPCId(2004354).Location) > 0.8)
                        {
                            MovementManager.SetFacing(GameObjectManager.GetObjectByNPCId(2004354).Location);
                            MovementManager.MoveForwardStart();
                            await Coroutine.Sleep(100);
                            MovementManager.MoveStop();
                        }
                    }
                    else
                    {
                        while (Core.Me.Location.Distance2D(PartyManager.VisibleMembers
                                   .Where(x => !x.IsMe && x.BattleCharacter.IsAlive &&
                                               x.BattleCharacter.Auras.Any(y => y.Id == 12)).FirstOrDefault()
                                   .BattleCharacter.Location) > 0.8)
                        {
                            MovementManager.SetFacing(PartyManager.VisibleMembers
                                .Where(x => !x.IsMe && x.BattleCharacter.IsAlive &&
                                            x.BattleCharacter.Auras.Any(y => y.Id == 12)).FirstOrDefault()
                                .BattleCharacter.Location);
                            MovementManager.MoveForwardStart();
                            await Coroutine.Sleep(100);
                            MovementManager.MoveStop();
                        }
                    }

                    //await Coroutine.Sleep(1000);
                    await Coroutine.Yield();
                }

                sw.Stop();
            }

            HashSet<uint> AncientFlare = new HashSet<uint>()
            {
                2317,
                1730,
                1731,
                1748,
                2347,
                5253,
                2359,
                11928
            };
            if (AncientFlare.IsCasting())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //Logging.Write(Colors.Aquamarine, $"Ancient Flare Handling");
                while (sw.ElapsedMilliseconds < 10000)
                {
                    Core.Me.ClearTarget();
                    while (Core.Me.Location.Distance2D(PartyManager.VisibleMembers
                               .Where(x => !x.IsMe && x.BattleCharacter.IsAlive &&
                                           x.BattleCharacter.Auras.Any(y => y.Id == 12)).FirstOrDefault()
                               .BattleCharacter.Location) > 0.5)
                    {
                        MovementManager.SetFacing(PartyManager.VisibleMembers
                            .Where(x => !x.IsMe && x.BattleCharacter.IsAlive &&
                                        x.BattleCharacter.Auras.Any(y => y.Id == 12)).FirstOrDefault().BattleCharacter
                            .Location);
                        MovementManager.MoveForwardStart();
                        await Coroutine.Sleep(100);
                        MovementManager.MoveStop();
                    }

                    await Coroutine.Sleep(1000);
                    await Coroutine.Yield();
                }

                sw.Stop();
            }

            #endregion

            /// Default (缺省)
            if (Spells.IsCasting())
            {
                Core.Me.ClearTarget(); //Logging.Write(Colors.Aquamarine, $"Default Spell Handling");
                while (Core.Me.Location.Distance2D(PartyManager.VisibleMembers
                           .Where(x => !x.IsMe && x.BattleCharacter.IsAlive).FirstOrDefault().BattleCharacter
                           .Location) > 0.5)
                {
                    MovementManager.SetFacing(PartyManager.VisibleMembers
                        .Where(x => !x.IsMe && x.BattleCharacter.IsAlive).FirstOrDefault().BattleCharacter.Location);
                    MovementManager.MoveForwardStart();
                    await Coroutine.Sleep(100);
                    MovementManager.MoveStop();
                }

                //await Coroutine.Sleep(1000);
                await Coroutine.Yield();
                ;
            }

            /// SideStep (回避)
            //if (WorldManager.SubZoneId != 2996) { Helpers.BossIds.ToggleSideStep(new uint[] { 8210 }); } else { Helpers.BossIds.ToggleSideStep(); }

            return false;
        }

        public override DungeonId DungeonId { get; }

    }
}
