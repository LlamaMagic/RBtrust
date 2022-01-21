﻿using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Helpers;

namespace Trust.Extensions
{
    /// <summary>
    /// Various extension methods.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Checks if any nearby <see cref="BattleCharacter"/> is casting any spell ID in this collection.
        /// </summary>
        /// <param name="spellCastIds">Spell IDs to check against.</param>
        /// <returns><see langword="true"/> if any given spell is being casted.</returns>
        public static bool IsCasting(this HashSet<uint> spellCastIds)
        {
            return GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                    .Any(obj => spellCastIds.Contains(obj.CastingSpellId) && obj.Distance() < 50);
        }

        /// <summary>
        /// Follows the specified <see cref="BattleCharacter"/>.
        /// </summary>
        /// <param name="bc">Character to follow.</param>
        /// <param name="followDistance">Distance to follow at.</param>
        /// <param name="msWait">Time between movement ticks, in milliseconds.</param>
        /// <param name="useMesh">Whether to use Nav Mesh or move blindly.</param>
        /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
        public static async Task<bool> Follow(this BattleCharacter bc, float followDistance = 0.3f, int msWait = 100, bool useMesh = false)
        {
            float curDistance = Core.Me.Location.Distance(bc.Location);

            if (bc == null)
            {
                return true;
            }

            if (curDistance < followDistance)
            {
                return true;
            }

            while (!Core.Me.IsDead)
            {
                curDistance = Core.Me.Location.Distance(bc.Location);

                if (curDistance < followDistance)
                {
                    break;
                }

                if (Core.Me.IsDead)
                {
                    return false;
                }

                if (Core.Me.IsCasting)
                {
                    ActionManager.StopCasting();
                }
#if RB_CN
                Logging.Write(Colors.Aquamarine, $"跟随 队友 {bc.Name} [距离: {Core.Me.Distance(bc.Location)}]");
#else
                Logging.Write(Colors.Aquamarine, $"Following {bc.Name} [Distance: {curDistance}]");
#endif
                if (useMesh)
                {
                    await CommonTasks.MoveTo(bc.Location);
                }
                else
                {
                    Navigator.PlayerMover.MoveTowards(bc.Location);
                }

                await Coroutine.Sleep(msWait);
            }

            return await StopMoving();
        }

        /// <summary>
        /// Stops the player's movement.
        /// </summary>
        /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
        public static async Task<bool> StopMoving()
        {
            if (!MovementManager.IsMoving)
            {
                return true;
            }

            int ticks = 0;
            while (MovementManager.IsMoving && ticks < 100)
            {
                Navigator.Stop();
                await Coroutine.Sleep(100);
                ticks++;
            }

            return true;
        }

        /// <summary>
        /// Disables SideStep around certain boss-related monsters.
        /// </summary>
        /// <param name="bossIds">Boss monster IDs.</param>
        /// <param name="ignoreIds">IDs to filter out of the base list.</param>
        public static void ToggleSideStep(this HashSet<uint> bossIds, uint[] ignoreIds = null)
        {
            if (Core.Target == null)
            {
                return;
            }

            PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

            if (sidestepPlugin != null)
            {
                HashSet<uint> filteredIds = new HashSet<uint>(bossIds.Where(id => ignoreIds == null || !ignoreIds.Contains(id)));

                bool isBoss = ignoreIds != null
                ? GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                                   .Any(obj => obj.Distance() < 50 && filteredIds.Contains(obj.NpcId))
                : GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                                   .Any(obj => obj.Distance() < 50 && bossIds.Contains(obj.NpcId));

                sidestepPlugin.Enabled = !isBoss;
            }
        }
    }
}
