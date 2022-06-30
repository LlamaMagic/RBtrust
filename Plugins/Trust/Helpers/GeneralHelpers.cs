using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using RBTrust.Plugins.Trust.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Trust.Helpers
{
    /// <summary>
    /// Miscellaneous functions
    /// </summary>
    internal static class GeneralHelpers
    {
        public static bool IsTankClass()
        {
            if (Core.Me.CurrentJob == ClassJobType.Paladin || Core.Me.CurrentJob == ClassJobType.Gladiator
                                                           || Core.Me.CurrentJob == ClassJobType.Warrior || Core.Me.CurrentJob == ClassJobType.Marauder
                                                           || Core.Me.CurrentJob == ClassJobType.DarkKnight || Core.Me.CurrentJob == ClassJobType.Gunbreaker)
            {
                return true;
            }

            return false;
        }

    }
}
