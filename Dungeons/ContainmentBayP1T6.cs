using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60: The Limitless Blue trial logic.
/// </summary>
public class ContainmentBayP1T6 : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.ContainmentBayP1T6;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ContainmentBayP1T6;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.ContainmentBayP1T6,
            () => ArenaCenter.Sophia,
            outerRadius: 90.0f,
            innerRadius: 17.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Sophia
        /// </summary>
        public const uint Sophia = 4776;

    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss: Sephirot.
        /// </summary>
        public static readonly Vector3 Sophia = new(0f, 0f, 0f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Pillar of Mercy
        /// When this spell casts he targets a party memeber that needs to hide behind a rock, since we can't tell who's targetting just hide
        /// </summary>
        public static readonly HashSet<uint> PillarofMercy = new() {5866,5579};
    }

    private static class AblityTimers
    {
    }
}
