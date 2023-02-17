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
/// Lv. 60: AlexanderA4BurdenoftheFather raid logic.
/// </summary>
public class AlexanderA4BurdenoftheFather : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AlexanderA4BurdenoftheFather;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AlexanderA4BurdenoftheFather;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.AlexanderA4BurdenoftheFather,
            () => ArenaCenter.LeftandRightForeleg,
            outerRadius: 90.0f,
            innerRadius: 24.2f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.AlexanderA4BurdenoftheFather,
            () => ArenaCenter.SmallPlatform,
            outerRadius: 90.0f,
            innerRadius: 7.7f,
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
        /// Boss: Left Foreleg
        /// </summary>
        public const uint LeftForeleg = 4347;

        /// <summary>
        /// Boss: Right Foreleg
        /// </summary>
        public const uint RightForeleg = 4346;
    }

    private static class ArenaCenter
    {
        public static readonly Vector3 LeftandRightForeleg = new(-0.03689127f, 10.6f, -0.03860924f);

        public static readonly Vector3 SmallPlatform = new(1021f, 38f, 16f);
    }



    private static class EnemyAction
    {

    }

    private static class AblityTimers
    {
    }
}
