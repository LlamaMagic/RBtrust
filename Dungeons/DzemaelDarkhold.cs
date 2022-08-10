using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Logging;
using ActionType = ff14bot.Enums.ActionType;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 44: Dzemael Darkhold dungeon logic.
/// </summary>
public class DzemaelDarkhold : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.DzemaelDarkhold;

    private const int AllseeingEye = 1397;
    private const int GlowingCrystal = 2000276;
    private const int Taulurd = 1415;
    private const int Batraal = 1396;

    private const int CrystalVeilAura = 322;
    private const int InvincibilityAura = 325;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // All-seeing Eye
        if (WorldManager.SubZoneId == (uint)SubZoneId.GrandHall && Core.Me.InCombat)
        {
            BattleCharacter boss = (BattleCharacter)GameObjectManager.GetObjectByNPCId(AllseeingEye);
            Aura invulnAura = boss.Auras.AuraList.FirstOrDefault(x => x.Id == InvincibilityAura);

            GameObject crystal = GameObjectManager.GetObjectsByNPCId<GameObject>(NpcId: GlowingCrystal)
                .FirstOrDefault(bc => bc.Distance() < 63 && bc.IsVisible);
            {
                if (boss.Auras.AuraList.Contains(invulnAura) && Core.Me.InCombat &&
                    GameObjectManager.Attackers.Contains(boss))
                {
                    while (!Core.Me.HasAura(CrystalVeilAura) && crystal.IsVisible)
                    {
                        CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Moving to crystal");
                        Logger.Information($"Moving to crystal.");
                        if (PartyManager.IsInParty && PartyManager.AllMembers.Any(pm => pm is TrustPartyMember))
                        {
                            Logger.Information($"Using Disengage.");
                            boss.Target();
                            ActionManager.DoAction(ActionType.Squadron, SquadronAction.Disengage, GameObjectManager.Target);
                        }

                        await LlamaLibrary.Helpers.Navigation.FlightorMove(crystal.Location);
                    }

                    await Coroutine.Wait(30_000, () => !boss.Auras.AuraList.Contains(invulnAura));
                    if (!boss.Auras.AuraList.Contains(invulnAura) && Core.Me.InCombat &&
                        GameObjectManager.Attackers.Contains(boss) && (PartyManager.IsInParty &&
                                                                       PartyManager.AllMembers.Any(pm =>
                                                                           pm is TrustPartyMember)))
                    {
                        Logger.Information($"Using Engage.");
                        boss.Target();
                        ActionManager.DoAction(ActionType.Squadron, SquadronAction.Engage, GameObjectManager.Target);
                    }
                }
            }
        }

        await Coroutine.Yield();
        return false;
    }
}
