using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 70: The Royal Menagerie.
/// </summary>
public class TheRoyalMenagerie : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheRoyalMenagerie;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheRoyalMenagerie;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.TheRoyalMenagerie,
            () => ArenaCenter.Shinryu,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => WorldManager.ZoneId == (uint)ZoneId.TheRoyalMenagerie,
            innerWidth: 10.0f,
            innerHeight: 10.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Shinryu },
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
        /// Boss: Shinryu.
        /// </summary>
        public const uint Shinryu = 5640;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Lakshmi.
        /// </summary>
        public static readonly Vector3 Shinryu = new(0f, -380f, 0f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Shinryu"/>'s the Pull of Light  .
        /// Tank buster
        ///
        /// </summary>
        public const uint thePullofLight = 9362;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Seduced
        /// </summary>
        public const uint Seduced = 1389;

        /// <summary>
        /// Vril
        /// </summary>
        public const uint Vril = 1290;
    }
}
