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
public class AlexanderA3ArmoftheFather : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AlexanderA3ArmoftheFather;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AlexanderA3ArmoftheFather;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.AlexanderA3ArmoftheFather,
            () => ArenaCenter.LivingLiquid,
            outerRadius: 90.0f,
            innerRadius: 22.0f,
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
        /// Boss: Living Liquid
        /// </summary>
        public const uint LivingLiquid = 100;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss: Sophia.
        /// </summary>
        public static readonly Vector3 LivingLiquid = new(58f, -8.99996f, -63f);
    }



    private static class EnemyAction
    {

    }

    private static class AblityTimers
    {
    }
}
