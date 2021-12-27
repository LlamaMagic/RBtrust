using Buddy.Coroutines;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 75 The Qitana Ravel dungeon logic.
    /// </summary>
    public class TheQitanaRavel : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Dungeons.ZoneId.TheQitanaRavel;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheQitanaRavel;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            HashSet<uint> spellCastIds = new HashSet<uint>()
            {
                15918, 15916, 15917, 17223, 15498, 15499,
                15500, 15725, 15501, 15502, 15503, 15504,
                15509, 15510, 15511, 15512, 15926, 17213,
                15570, 16263, 14730, 16260, 15514, 15516,
                15517, 15518, 15519, 15520, 16923, 15524,
                15523, 15527, 15522, 15526, 15521, 15525,
            };

            bool spellCast = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false).Where(obj =>
                spellCastIds.Contains(obj.CastingSpellId) && obj.Distance() < 50).Count() > 0;

            if (spellCast)
            {
                HashSet<string> partyMemberNames = new HashSet<string>() { /*"桑克瑞德",*/ "雅·修特拉", "于里昂热", "阿尔菲诺", "阿莉塞", "雅·修特拉", /*"水晶公",*/ "琳", "敏菲利亚", "莱楠" };
                HashSet<uint> partyMemberIds = new HashSet<uint>() { /*713,*/ 729, 1492, 4130, 5239, 8378, /*8650,*/ 8889, 8917, 8919, 11264, 11265, 11268, 11269, 11270 };
#if RB_CN
				var closest = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false).Where(obj =>
                partyMemberNames.Contains(obj.Name) && !obj.IsDead).OrderBy(r => r.Distance()).FirstOrDefault();
#else
                BattleCharacter closest = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false).Where(obj =>
                partyMemberIds.Contains(obj.NpcId) && !obj.IsDead).OrderBy(r => r.Distance()).FirstOrDefault();
#endif

                if (Core.Me.Distance(closest.Location) >= 0.3)
                {
                    if (Core.Me.IsCasting)
                    {
                        ActionManager.StopCasting();
                    }
#if RB_CN
                    Logging.Write(Colors.Aquamarine, $"跟随 队友 {closest.Name} [距离: {Core.Me.Distance(closest.Location)}]");
#else
                    Logging.Write(Colors.Aquamarine, $"Following {closest.Name} [Distance: {Core.Me.Distance(closest.Location)}]");
#endif
                    while (Core.Me.Distance(closest.Location) >= 0.3)
                    {
                        Navigator.PlayerMover.MoveTowards(closest.Location);
                        await Coroutine.Sleep(100);
                    }

                    Navigator.PlayerMover.MoveStop();
                    await Coroutine.Sleep(100);

                    return true;
                }
            }

            if (Core.Target != null)
            {
                PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

                IEnumerable<BattleCharacter> isBoss = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false).Where(r => r.Distance() < 50 &&
                    (r.NpcId == 8231 || r.NpcId == 8232 || r.NpcId == 8233));

                if (isBoss.Any())
                {
                    if (sidestepPlugin != null)
                    {
                        if (sidestepPlugin.Enabled)
                        {
                            sidestepPlugin.Enabled = false;
                        }
                    }
                }
            }

            return false;
        }
    }
}
