using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Abstract starting point for implementing specialized dungeon logic.
/// </summary>
public abstract class AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public abstract ZoneId ZoneId { get; }

    /// <summary>
    /// Gets <see cref="DungeonId"/> for this dungeon.
    /// </summary>
    public abstract DungeonId DungeonId { get; }

    /// <summary>
    /// Gets a handle to signal the combat routine should not use certain features (e.g., prevent CR from moving).
    /// </summary>
    protected CapabilityManagerHandle CapabilityHandle { get; } = CapabilityManager.CreateNewHandle();

    /// <summary>
    /// Gets SideStep Plugin reference.
    /// </summary>
    protected PluginContainer SidestepPlugin { get; } = PluginHelpers.GetSideStepPlugin();

    /// <summary>
    /// Gets spell IDs to follow-dodge while any contained spell is casting.
    /// </summary>
    protected abstract HashSet<uint> SpellsToFollowDodge { get; }

    /// <summary>
    /// Setup -- run once after entering the dungeon.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public virtual Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();
        SidestepPlugin.Enabled = true;

        return Task.FromResult(false);
    }

    /// <summary>
    /// Tear-down -- run once after exiting the dungeon.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public virtual Task<bool> OnExitDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();
        SidestepPlugin.Enabled = true;

        return Task.FromResult(false);
    }

    /// <summary>
    /// Executes dungeon logic.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public abstract Task<bool> RunAsync();

    /// <summary>
    /// Follows closest safe ally while <see cref="SpellsToFollowDodge"/> are casting.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    protected async Task<bool> FollowDodgeSpells()
    {
        if (SpellsToFollowDodge == null || SpellsToFollowDodge.Count == 0)
        {
            return false;
        }

        BattleCharacter caster = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
            .FirstOrDefault(bc => SpellsToFollowDodge.Contains(bc.CastingSpellId));

        if (caster != null)
        {
            SpellCastInfo spell = caster.SpellCastInfo;
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, spell.RemainingCastTime, $"Follow-Dodge: ({caster.NpcId}) {caster.Name} is casting ({spell.ActionId}) {spell.Name} for {spell.RemainingCastTime.TotalMilliseconds:N0}ms");

            await MovementHelpers.GetClosestAlly.Follow();
        }

        return false;
    }
}
