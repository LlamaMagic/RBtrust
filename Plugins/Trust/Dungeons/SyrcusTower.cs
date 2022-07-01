using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 50: Syrcus Tower dungeon logic.
    /// </summary>
    public class SyrcusTower : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.SyrcusTower;

        private static readonly HashSet<uint> Spells = new HashSet<uint>()
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

        private static readonly HashSet<uint> CurtainCall = new HashSet<uint>() { 2441, 12461 };
        private static readonly HashSet<uint> AncientQuaga = new HashSet<uint>() { 2361, 12214, 3412, 4198, 5254, 5253, 2359, 3413, };
        private static readonly HashSet<uint> AncientFlare = new HashSet<uint>() { 2317, 1730, 1731, 1748, 2347, 5253, 2359, 11928, };

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.NONE;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            // NOT TESTED
            if (CurtainCall.IsCasting())
            {
                await Coroutine.Sleep(4000);
                Stopwatch sw = new Stopwatch();
                sw.Start();

                while (sw.ElapsedMilliseconds < 12000)
                {
                    Core.Me.ClearTarget();

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

                    await Coroutine.Yield();
                }

                sw.Reset();
            }

            if (AncientQuaga.IsCasting())
            {
                await Coroutine.Sleep(2000);
                Stopwatch sw = new Stopwatch();
                sw.Start();

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

                    await Coroutine.Yield();
                }

                sw.Reset();
            }

            if (AncientFlare.IsCasting())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

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

            if (Spells.IsCasting())
            {
                Core.Me.ClearTarget();
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

                await Coroutine.Yield();
            }

            return false;
        }
    }
}
