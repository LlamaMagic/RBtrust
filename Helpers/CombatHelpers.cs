using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Trust.Logging;

namespace Trust.Helpers;

internal static class CombatHelpers
{
    private static uint reprisal = 7535;
    private static uint rampart = 7531;

    /// <summary>
    /// Logic for handling tank busters
    /// </summary>
    internal static async Task<bool> HandleTankBuster()
    {
        if (ActionManager.CanCast(rampart, Core.Player))
        {
            SpellData action = DataManager.GetSpellData(rampart);
            Logger.Information($"Casting {action.Name} ({action.Id})");
            ActionManager.DoAction(action, Core.Player);
            await Coroutine.Sleep(1_500);
        }

        if (ActionManager.CanCast(reprisal, Core.Player.CurrentTarget))
        {
            SpellData action = DataManager.GetSpellData(reprisal);
            Logger.Information($"Casting {action.Name} ({action.Id})");
            ActionManager.DoAction(action, Core.Player.CurrentTarget);
            await Coroutine.Sleep(1_500);
        }

        return false;
    }
}
