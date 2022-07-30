using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 15.3: Hall of the Novice: Final dungeon logic.
/// </summary>
public class HallOfTheNoviceWesternLa : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.HallOfTheNoticeWesternLa;

    private const int QuickFistedTrainingPartner = 4786;
    private const int TamedJackal = 4787;

    private const float AttackRange = 20.0f;
    private static readonly Dictionary<ClassJobType, uint> JobSpecificAction = new()
    {
        { ClassJobType.Marauder, 46 },
        { ClassJobType.Warrior, 46 },
        { ClassJobType.Gladiator, 24 },
        { ClassJobType.Paladin, 24 },
        { ClassJobType.DarkKnight, 3624 },
        { ClassJobType.Gunbreaker, 16143 },
    };

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        IEnumerable<BattleCharacter> targets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
            .Where(bc => bc.NpcId == QuickFistedTrainingPartner || bc.NpcId == TamedJackal)
            .Where(bc => bc.IsAlive && bc.CurrentHealth > 1)
            .Where(bc => bc.IsVisible && bc.IsTargetable)
            .OrderBy(bc => bc.Distance());

        foreach (BattleCharacter target in targets)
        {
            while (target.TargetGameObject != Core.Me)
            {
                Logger.Information($"Drawing aggro of {target.Name} ({target.NpcId})");

                if (target.Distance2D(Core.Me.Location) >= AttackRange)
                {
                    Vector3 location = target.Location;
                    Navigator.PlayerMover.MoveTowards(location);
                    while (location.Distance2D(Core.Me.Location) >= AttackRange)
                    {
                        Navigator.PlayerMover.MoveTowards(location);
                        await Coroutine.Sleep(100);
                    }

                    Navigator.PlayerMover.MoveStop();
                }

                if (JobSpecificAction.TryGetValue(Core.Player.CurrentJob, out uint actionId))
                {
                    SpellData action = DataManager.GetSpellData(actionId);
                    Logger.Information($"Casting {action.Name} ({action.Id}) on {target}");
                    ActionManager.DoAction(action, target);
                    await Coroutine.Sleep(1_500);
                }
            }
        }

        await Coroutine.Yield();

        return false;
    }
}
