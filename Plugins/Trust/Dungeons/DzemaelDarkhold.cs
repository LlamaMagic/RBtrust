using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using ff14bot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;
using ActionType = ff14bot.Enums.ActionType;

namespace Trust.Dungeons
{
    /// <summary>
    /// Abstract starting point for implementing specialized dungeon logic.
    /// </summary>
    public class DzemaelDarkhold : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.DzemaelDarkhold;

        public override DungeonId DungeonId => DungeonId.NONE;

        private const int AllseeingEye = 1397;
        private const int glowingCrystal = 2000276;
        private const int Taulurd = 1415;
        private const int Batraal = 1396;

        /// <summary>
        /// Gets a handle to signal the combat routine should not use certain features (e.g., prevent CR from moving).
        /// </summary>
        protected CapabilityManagerHandle CapabilityHandle { get; } = CapabilityManager.CreateNewHandle();

        /// <summary>
        /// Gets SideStep Plugin reference.
        /// </summary>
        protected PluginContainer SidestepPlugin { get; } = PluginHelpers.GetSideStepPlugin();

        /// <summary>
        /// Executes dungeon logic.
        /// </summary>
        /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
        public override async Task<bool> RunAsync()
        {
            // All-seeing Eye
            if (WorldManager.SubZoneId == (uint)SubZoneId.GrandHall && Core.Me.InCombat)
            {
                BattleCharacter boss = (BattleCharacter)GameObjectManager.GetObjectByNPCId(AllseeingEye);
                Aura invulnAura = boss.Auras.AuraList.FirstOrDefault(x => x.Id == 325);

                GameObject crystal = GameObjectManager.GetObjectsByNPCId<GameObject>(NpcId: glowingCrystal)
                    .FirstOrDefault(bc => bc.Distance() < 63 && bc.IsVisible);
                {
                    if (boss.Auras.AuraList.Contains(invulnAura) && Core.Me.InCombat &&
                        GameObjectManager.Attackers.Contains(boss))
                    {
                        while (!Core.Me.HasAura(322) && crystal.IsVisible) // Crystal Veil
                        {
                            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000,
                                "Moving to crystal");
                            Logging.Write($"Moving to crystal.");
                            if (PartyManager.IsInParty && PartyManager.AllMembers.Any(pm => pm is TrustPartyMember))
                            {
                                Logging.Write($"Using Disengage.");
                                boss.Target();
                                ActionManager.DoAction(ActionType.Squadron, 2, GameObjectManager.Target);
                            }

                            await LlamaLibrary.Helpers.Navigation.FlightorMove(crystal.Location);
                        }

                        await Coroutine.Wait(30000, () => !boss.Auras.AuraList.Contains(invulnAura));
                        if (!boss.Auras.AuraList.Contains(invulnAura) && Core.Me.InCombat &&
                            GameObjectManager.Attackers.Contains(boss) && (PartyManager.IsInParty &&
                                                                           PartyManager.AllMembers.Any(pm =>
                                                                               pm is TrustPartyMember)))
                        {
                            Logging.Write($"Using Engage.");
                            boss.Target();
                            ActionManager.DoAction(ActionType.Squadron, 1, GameObjectManager.Target);
                        }
                    }
                }
            }

            await Coroutine.Yield();
            return false;
        }
    }
}
